﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Registration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Nancy.Bootstrappers.Mef
{

    /// <summary>
    /// Pre-configured registration builder.
    /// </summary>
    class NancyRegistrationBuilder : RegistrationBuilder
    {

        /// <summary>
        /// Provides a factory implementation for Func imports. MEF doesn't support resolving Funcs directly into a
        /// factory, so this class exports a Func contract which acts as an export factory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class FuncFactory<T>
            where T : class
        {

            ExportFactory<T> factory;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="factory"></param>
            public FuncFactory(ExportFactory<T> factory)
            {
                Contract.Requires<ArgumentNullException>(factory != null);

                this.factory = factory;
            }

            /// <summary>
            /// Implements the TinyIoC factory method.
            /// </summary>
            /// <returns></returns>
            public Func<T> ExportFunc
            {
                get { return () => factory.CreateExport().Value; }
            }

        }

        /// <summary>
        /// Reference to ExportFunc property on generic definition of <see cref="FuncFactory"/>.
        /// </summary>
        public static readonly PropertyInfo ExportFuncPropertyInfo = typeof(FuncFactory<>).GetProperty("ExportFunc");

        /// <summary>
        /// Set of functions which filter out bad types.
        /// </summary>
        static readonly Func<Type, bool>[] constraints = new Func<Type, bool>[]
        {
            t => !typeof(Exception).IsAssignableFrom(t),
        };

        /// <summary>
        /// Returns <c>true</c> if the given type is an exportable part.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static internal bool IsExportablePart(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            var b = true;

            // parts must be classes, and not abstract, public with at least one constructor
            if (!(b &= type.IsClass && !type.IsAbstract && type.IsPublic && type.GetConstructors().Length > 0))
                return false;

            // type must be in assembly which references Nancy, or at least a Nancy prefixed component
            if (!(b &= TypeHelpers.ReferencesNancy(type)))
                return false;

            // if the type has any MEF Export attributes on it, we should ignore it; it should be handled by a standard
            // MEF catalog
            if (type.IsClass)
            {
                var referencesMef = type.Assembly.GetReferencedAssemblies()
                    .Any(i => i.Name == typeof(ExportAttribute).Assembly.GetName().Name);

                var exports = type
                    .Recurse(i => i.BaseType)
                    .SelectMany(i => i.GetMembers(BindingFlags.Instance | BindingFlags.Public).Prepend(i))
                    .SelectMany(i => i.GetCustomAttributes<ExportAttribute>())
                    .ToDebugList();

                if (!(b &= !(referencesMef && exports.Any())))
                    return false;
            }

            return b;
        }

        /// <summary>
        /// Returns <c>true</c> if the given interface type is exportable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static internal bool IsExportableInterface(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            var b = true;

            if (!(b &= type.IsInterface && type.IsPublic))
                return false;

            // type must be in assembly which references Nancy, or at least a Nancy prefixed component
            if (!(b &= TypeHelpers.ReferencesNancy(type)))
                return false;

            // evaluate other arbitrary constraints
            var passed = constraints.All(i => i(type));
            if (!(b &= passed))
                return false;

            return true;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public NancyRegistrationBuilder()
            : base()
        {
            // export FuncFactory as an open generic instance; but more importantly, export its CreateExport function
            // as an appropriate Func<T> contract.
            ForType(typeof(FuncFactory<>))
                .AddMetadata(NancyMetadataKeys.RegistrationBuilder, this)
                .AddMetadata(NancyMetadataKeys.Part, true)
                .AddMetadata(NancyMetadataKeys.InternalPart, true)
                .Export(i => i
                    .AddMetadata(NancyMetadataKeys.RegistrationBuilder, this)
                    .AddMetadata(NancyMetadataKeys.Export, true)
                    .AddMetadata(NancyMetadataKeys.SelfExport, true)
                    .AddMetadata(NancyMetadataKeys.InternalExport, true)
                    .AsContractType(typeof(FuncFactory<>)))
                .ExportProperties(i =>
                    i == ExportFuncPropertyInfo, (i, j) => j
                        .AddMetadata(NancyMetadataKeys.RegistrationBuilder, this)
                        .AddMetadata(NancyMetadataKeys.Export, true)
                        .AddMetadata(NancyMetadataKeys.InternalExport, true)
                        .AsContractType(typeof(Func<>))
                        .AsContractName("FuncFactory:" + AttributedModelServices.GetContractName(typeof(Func<>))));

            // export any exportable parts
            ForTypesMatching(i => IsExportablePart(i))
                .AddMetadata(NancyMetadataKeys.RegistrationBuilder, this)
                .AddMetadata(NancyMetadataKeys.Part, true)
                .Export(i => i
                    .AddMetadata(NancyMetadataKeys.RegistrationBuilder, this)
                    .AddMetadata(NancyMetadataKeys.Export, true)
                    .AddMetadata(NancyMetadataKeys.SelfExport, true))
                .ExportInterfaces(i =>
                    IsExportableInterface(i), (i, j) => j
                        .AddMetadata(NancyMetadataKeys.RegistrationBuilder, this)
                        .AddMetadata(NancyMetadataKeys.Export, true))
                .SelectConstructor(i =>
                    SelectConstructor(i), (i, j) =>
                        BuildParameter(i, j));
        }

        /// <summary>
        /// Returns the preferred constructor.
        /// </summary>
        /// <param name="constructors"></param>
        /// <returns></returns>
        ConstructorInfo SelectConstructor(ConstructorInfo[] constructors)
        {
            Contract.Requires<ArgumentNullException>(constructors != null);

            // TODO can we do something here about selecting one based on exports? Hmm.
            return constructors
                .OrderByDescending(i => i.GetParameters().Length)
                .FirstOrDefault();
        }

        /// <summary>
        /// Builds the given parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="builder"></param>
        void BuildParameter(ParameterInfo parameter, ImportBuilder builder)
        {
            Contract.Requires<ArgumentNullException>(parameter != null);
            Contract.Requires<ArgumentNullException>(builder != null);

            var name = parameter.ParameterType.FullName;

            {
                // decides whether the parameter is attempting to import many items

                Type importManyType = null;

                if (parameter.ParameterType.IsGenericType() &&
                    parameter.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    importManyType = parameter.ParameterType.GetGenericArguments()[0];

                if (parameter.ParameterType.IsGenericType() &&
                    parameter.ParameterType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    importManyType = parameter.ParameterType.GetGenericArguments()[0];

                if (parameter.ParameterType.IsArray)
                    importManyType = parameter.ParameterType.GetElementType();

                if (importManyType != null)
                {
                    builder.AsMany(true);
                    builder.AsContractType(importManyType);
                    return;
                }
            }

            {
                // decides whether the parameter is attempting to import a Func<T>; essentially an ExportFactory

                Type funcType = null;

                if (parameter.ParameterType.IsGenericType() &&
                    parameter.ParameterType.GetGenericArguments().Length == 1 &&
                    parameter.ParameterType.GetGenericTypeDefinition() == typeof(Func<>))
                    funcType = parameter.ParameterType.GetGenericArguments()[0];

                if (funcType != null)
                {
                    // import a Func generated by our FuncFactory
                    var contractType = typeof(Func<>).MakeGenericType(funcType);
                    var contractName = "FuncFactory:" + AttributedModelServices.GetContractName(typeof(Func<>));
                    builder.AsContractType(contractType);
                    builder.AsContractName(contractName);
                    builder.AsMany(false);
                    return;
                }
            }

            // fall back to normal method
            builder.AsMany(false);
            builder.AsContractType(parameter.ParameterType);
            return;
        }

    }

}