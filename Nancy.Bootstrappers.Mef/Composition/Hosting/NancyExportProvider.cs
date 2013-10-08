using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.Contracts;
using System.Linq;
using Nancy.ViewEngines;

namespace Nancy.Bootstrappers.Mef.Composition.Hosting
{

    /// <summary>
    /// Provides Nancy exports for a MEF container. Nancy requires that we deal with multiple implementations of the
    /// same contract, as well as support adding exports on the fly.
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

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public NancyExportProvider()
            : this(Enumerable.Empty<ExportProvider>())
        {

        }

        protected override IEnumerable<Export> GetExportsCoreExactlyOne(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            // replace ZeroOrOne with ZeroOrMore to prevent errors down the chain; we'll grab the first
            var export = GetExportsCoreZeroOrMore(new ImportDefinition(
                    definition.Constraint,
                    definition.ContractName,
                    ImportCardinality.ZeroOrMore,
                    definition.IsRecomposable,
                    definition.IsPrerequisite,
                    definition.Metadata), atomicComposition)
                .FirstOrDefault();
            if (export != null)
                yield return export;

            if (definition.ContractName == AttributedModelServices.GetContractName(typeof(IFileSystemReader)))
                yield return new Export(definition.ContractName, () => new DefaultFileSystemReader());
        }

    }

}
