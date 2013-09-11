using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Nancy.Bootstrappers.Mef.Composition.Hosting
{

    /// <summary>
    /// Wraps the user provided <see cref="CompositionContainer"/> in order to implement Nancy specific policy, such
    /// as delivery of single-instance imports though multiple are available.
    /// </summary>
    public class NancyExportProvider : DynamicAggregateExportProvider
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="providers"></param>
        public NancyExportProvider(IEnumerable<ExportProvider> providers)
            : base(providers)
        {
            Contract.Requires<ArgumentNullException>(providers != null);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="providers"></param>
        public NancyExportProvider(params ExportProvider[] providers)
            : base(providers)
        {
            Contract.Requires<ArgumentNullException>(providers != null);
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            // replace ZeroOrOne with ZeroOrMore to prevent errors down the chain; we'll grab the first
            if (definition.Cardinality == ImportCardinality.ZeroOrOne ||
                definition.Cardinality == ImportCardinality.ExactlyOne)
                return base.GetExportsCore(new ImportDefinition(
                    definition.Constraint,
                    definition.ContractName,
                    ImportCardinality.ZeroOrMore,
                    definition.IsRecomposable,
                    definition.IsPrerequisite,
                    definition.Metadata), atomicComposition)
                    .Take(1);

            return base.GetExportsCore(definition, atomicComposition);
        }

    }

}
