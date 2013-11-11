using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Registration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using Nancy.Bootstrappers.Mef.Composition.Hosting;
using Nancy.Bootstrappers.Mef.Extensions;

namespace Nancy.Bootstrappers.Mef.Composition.Registration
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
        static internal bool IsNancyPart(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            // we are a MEF assembly, we aren't Nancy parts
            if (type.Assembly == typeof(NancyBootstrapper).Assembly)
                return false;

            // parts must be classes, and not abstract, public with at least one constructor
            if (!(type.IsClass && !type.IsAbstract && type.IsPublic && type.GetConstructors().Length > 0))
                return false;

            // type must be in assembly which references Nancy, or at least a Nancy prefixed component
            if (!(TypeHelper.ReferencesNancy(type)))
                return false;

            // evaluate other arbitrary constraints
            var passed = constraints.All(i => i(type));
            if (!passed)
                return false;

            // if the type has any MEF Export attributes on it, we should ignore it; it should be handled by a standard
            // MEF catalog
            var referencesMef = type.Assembly.GetReferencedAssemblies()
                .Any(i => i.Name == typeof(ExportAttribute).Assembly.GetName().Name);

            var exports = type
                .Recurse(i => i.BaseType)
                .SelectMany(i => i.GetMembers(BindingFlags.Instance | BindingFlags.Public).Prepend(i))
                .SelectMany(i => i.GetCustomAttributes<ExportAttribute>())
                .ToDebugList();

            if (!(!referencesMef || !exports.Any()))
                return false;

            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the given interface type is exportable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static internal bool IsNancyContract(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            var b = true;

            if (!(b &= type.IsInterface && type.IsPublic))
                return false;

            // type must be in assembly which references Nancy, or at least a Nancy prefixed component
            if (!(b &= TypeHelper.ReferencesNancy(type)))
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
                .Export(i => i
                    .AsContractType(typeof(FuncFactory<>)))
                .ExportProperties(i =>
                    i == ExportFuncPropertyInfo, (i, j) => j
                        .AsContractType(typeof(Func<>))
                        .AsContractName("FuncFactory:" + AttributedModelServices.GetContractName(typeof(Func<>))));

            // export any exportable parts
            ForTypesMatching(i => IsNancyPart(i))
                .AddMetadata(NancyMetadataKeys.RegistrationBuilder, this)
                .Export()
                .ExportInterfaces(i => IsNancyContract(i), (i, j) => j
                    .AsContractType(i))
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

                if (parameter.ParameterType.IsGenericType &&
                    parameter.ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    importManyType = parameter.ParameterType.GetGenericArguments()[0];

                if (parameter.ParameterType.IsGenericType &&
                    parameter.ParameterType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    importManyType = parameter.ParameterType.GetGenericArguments()[0];

                if (parameter.ParameterType.IsArray)
                    importManyType = parameter.ParameterType.GetElementType();

                if (importManyType != null)
                {
                    builder.AsContractType(importManyType);
                    builder.AsMany(true);
                    builder.AllowDefault();
                    return;
                }
            }

            {
                // decides whether the parameter is attempting to import a Func<T>; essentially an ExportFactory

                Type funcType = null;

                if (parameter.ParameterType.IsGenericType &&
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
                    builder.AllowDefault();
                    return;
                }
            }

            // fall back to normal method
            builder.AsContractType(parameter.ParameterType);
            builder.AsMany(false);
            return;
        }

    }

}
