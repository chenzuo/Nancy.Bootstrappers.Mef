using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Nancy.Bootstrappers.Mef
{

    static class TypeHelpers
    {

        /// <summary>
        /// Returns <c>true</c> if the type references Nancy in some way.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ReferencesNancy(Type type)
        {
            Contract.Requires<ArgumentNullException>(type != null);

            return type.Assembly.GetReferencedAssemblies()
                .Prepend(type.Assembly.GetName())
                .Any(r => r.Name.StartsWith("Nancy", StringComparison.OrdinalIgnoreCase));
        }

    }

}
