using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Nancy.Bootstrappers.Mef.Composition
{

    static class CatalogUtils
    {

        /// <summary>
        /// GetExports implementation for Nancy catalogs.
        /// </summary>
        /// <param name="definition"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(
            Func<ImportDefinition, IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>>> getExports,
            ImportDefinition definition)
        {
            // replace ZeroOrOne with ZeroOrMore to prevent errors down the chain; we'll grab the first
            if (definition.Cardinality == ImportCardinality.ZeroOrOne ||
                definition.Cardinality == ImportCardinality.ExactlyOne)
                return getExports(new ImportDefinition(
                    definition.Constraint,
                    definition.ContractName,
                    ImportCardinality.ZeroOrMore,
                    definition.IsRecomposable,
                    definition.IsPrerequisite,
                    definition.Metadata))
                    .Take(1);

            return getExports(definition);
        }

    }

}
