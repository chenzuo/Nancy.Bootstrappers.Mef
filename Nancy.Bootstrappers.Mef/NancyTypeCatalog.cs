using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;

namespace Nancy.Bootstrappers.Mef
{

    /// <summary>
    /// Type catalog that provides export of Nancy implementations that are not decorated with standard
    /// MEF attributes. The <see cref="NancyReflectionContext"/> is used to virtualize MEF attributes.
    /// </summary>
    public class NancyTypeCatalog : TypeCatalog
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="types"></param>
        public NancyTypeCatalog(params Type[] types)
            : base(types, new NancyReflectionContext())
        {
            Contract.Requires<NullReferenceException>(types != null);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="types"></param>
        public NancyTypeCatalog(IEnumerable<Type> types)
            : base(types, new NancyReflectionContext())
        {
            Contract.Requires<NullReferenceException>(types != null);
        }

    }

}
