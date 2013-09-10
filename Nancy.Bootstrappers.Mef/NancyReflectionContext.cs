using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Context;

namespace Nancy.Bootstrappers.Mef
{

    /// <summary>
    /// Provides custom attributes for Nancy types that are not decorated with MEF attributes.
    /// </summary>
    public class NancyReflectionContext : CustomReflectionContext
    {

        /// <summary>
        /// We only handle types which either are in Nancy, or extend types in Nancy.
        /// </summary>
        static readonly Assembly nancyAssembly = typeof(NancyEngine).Assembly;

        /// <summary>
        /// Set of types that should not be exported, including any that inherit from them.
        /// </summary>
        static readonly Type[] blacklist = new Type[]
        {
            typeof(Exception),
        };

        /// <summary>
        /// Caches whether a given type is a contract.
        /// </summary>
        static ConcurrentDictionary<Type, bool> contracts =
            new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Caches all the attributes for an object.
        /// </summary>
        static ConcurrentDictionary<object, IEnumerable<object>> attributes =
            new ConcurrentDictionary<object, IEnumerable<object>>();

        /// <summary>
        /// Returns <c>true</c> if the type is related to Nancy in some way.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static bool IsNancyType(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            var name = type.Name;

            // check for with MEF ExportAttributes
            if (type.IsClass)
            {
                var mef = type
                    .Recurse(i => i.BaseType)
                    .Recurse(i => i.GetInterfaces())
                    .SelectMany(i => i.GetMembers(BindingFlags.Instance | BindingFlags.Public).Prepend(i))
                    .SelectMany(i => i.GetCustomAttributes<ExportAttribute>())
                    .ToDebugList();
                if (mef.Any())
                    return false;
            }

            // all related types
            var deps = type
                .Recurse(i => i.BaseType)
                .Concat(type.GetInterfaces())
                .SelectMany(i => (i.IsGenericType() ? i.GetGenericArguments() : Enumerable.Empty<Type>()).Prepend(i))
                .ToDebugList();

            // type must in some way depend on Nancy
            if (!deps.Any(i => i.Assembly == nancyAssembly))
                return false;

            // guess we're good
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the given type is an exportable contract.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        internal static bool IsExportableContract(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return contracts.GetOrAdd(type.UnderlyingSystemType, t =>
                (t.IsInterface || (t.IsClass && !t.IsAbstract)) && 
                t.IsPublic &&
                blacklist.All(i => !i.IsAssignableFrom(t)) &&
                IsNancyType(t));
        }

        /// <summary>
        /// Returns <c>true</c> if the given type is exportable part.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsExportablePart(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return type.IsClass && IsExportableContract(type);
        }

        /// <summary>
        /// Returns the appropriate Import attribute for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Attribute GetImportAttribute(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            // check for supported collection types
            var openType = type.IsGenericType() ? type.GetGenericTypeDefinition() : type;
            if (openType == typeof(IEnumerable) ||
                openType == typeof(IEnumerable<>) ||
                openType == typeof(ICollection) ||
                openType == typeof(ICollection<>) ||
                type.IsArray)
                return new ImportManyAttribute();

            // otherwise, it must be a normal import
            return new ImportAttribute();
        }

        /// <summary>
        /// Returns the Nancy contracts implemented by the specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetExportableContractTypes(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return type
                .Recurse(i => i.GetInterfaces())
                .Where(i => IsExportableContract(i));
        }

        /// <summary>
        /// Returns the <see cref="ConstructorInfo"/> for the given type that should be used to create the instance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static ConstructorInfo GetImportConstructor(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            // TODO for now we choose the one with the most parameters
            // TODO ideally we would instead implement an ExportProvider that picked the best
            return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(i => i.GetParameters().Length)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the custom attributes for the given <see cref="MemberInfo"/>, including also the <see cref="Type"/> of
        /// which the member info is declared on.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        static IEnumerable<object> GetCustomAttributes(Type type, MemberInfo member)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            Contract.Requires<ArgumentNullException>(member != null);

            if (member is Type)
                return GetCustomAttributes((Type)member);
            if (member is ConstructorInfo)
                return GetCustomAttributes(type, (ConstructorInfo)member);

            return Enumerable.Empty<object>();
        }

        /// <summary>
        /// Provides additional attributes for a Nancy export.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static IEnumerable<object> GetCustomAttributes(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            // export as each available contract type
            foreach (var contractType in GetExportableContractTypes(type.UnderlyingSystemType))
                yield return new NancyExportAttribute(contractType.UnderlyingSystemType);
        }

        /// <summary>
        /// Provides additional attributes for a Nancy export constructor.
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns></returns>
        static IEnumerable<object> GetCustomAttributes(Type type, ConstructorInfo constructor)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            Contract.Requires<ArgumentNullException>(constructor != null);

            // we are the matching constructor
            if (GetImportConstructor(type) == constructor)
                yield return new ImportingConstructorAttribute();
        }

        /// <summary>
        /// Provides additional attributes for a Nancy constructor parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        static IEnumerable<object> GetCustomAttributes(Type type, ParameterInfo parameter)
        {
            Contract.Requires<ArgumentNullException>(type != null);
            Contract.Requires<ArgumentNullException>(parameter != null);

            // we are on the matching constructor
            if (GetImportConstructor(type) == (ConstructorInfo)parameter.Member)
                yield return GetImportAttribute(parameter.ParameterType);
        }

        protected override IEnumerable<object> GetCustomAttributes(MemberInfo member, IEnumerable<object> declaredAttributes)
        {
            Contract.Requires<ArgumentNullException>(member != null);
            Contract.Requires<ArgumentNullException>(declaredAttributes != null);

            return attributes.GetOrAdd(member, m =>
            {
                var type = member as Type ?? member.DeclaringType;

                // return base attributes and our own
                return base.GetCustomAttributes(member, declaredAttributes)
                    .Concat(IsExportablePart(type.UnderlyingSystemType) ? GetCustomAttributes(type.UnderlyingSystemType, member) : Enumerable.Empty<object>())
                    .ToList();
            });
        }

        protected override IEnumerable<object> GetCustomAttributes(ParameterInfo parameter, IEnumerable<object> declaredAttributes)
        {
            Contract.Requires<ArgumentNullException>(parameter != null);
            Contract.Requires<ArgumentNullException>(declaredAttributes != null);

            return attributes.GetOrAdd(parameter, _ =>
            {
                var type = parameter.Member.ReflectedType;

                // return base attributes and our own
                return base.GetCustomAttributes(parameter, declaredAttributes)
                    .Concat(IsExportablePart(type.UnderlyingSystemType) ? GetCustomAttributes(type.UnderlyingSystemType, parameter) : Enumerable.Empty<object>())
                    .ToList();
            });
        }

    }

}
