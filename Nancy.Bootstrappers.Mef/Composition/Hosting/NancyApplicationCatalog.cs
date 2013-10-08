using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using Nancy.Bootstrappers.Mef.Composition.Registration;

namespace Nancy.Bootstrappers.Mef.Composition.Hosting
{

    /// <summary>
    /// MEF catalog that provides export of Nancy implementations that are not decorated with standard
    /// MEF attributes. The <see cref="NancyReflectionContext"/> is used to virtualize MEF attributes.
    /// </summary>
    public class NancyApplicationCatalog : ApplicationCatalog
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="types"></param>
        public NancyApplicationCatalog()
            : base(new NancyReflectionContext())
        {

        }

        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            //return CatalogUtils.GetExports(base.GetExports, definition);
            return base.GetExports(definition);
        }

    }

}
