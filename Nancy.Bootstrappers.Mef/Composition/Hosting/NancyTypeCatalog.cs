using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;

using Nancy.Bootstrappers.Mef.Composition.Registration;

namespace Nancy.Bootstrappers.Mef.Composition.Hosting
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

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            //return CatalogUtils.GetExports(base.GetExports, definition);
            return base.GetExports(definition);
        }

    }

}
