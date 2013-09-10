using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics.Contracts;

namespace Nancy.Bootstrappers.Mef
{

    public abstract class NancyCatalog : FilteredCatalog
    {

        class FilterCache
        {

            /// <summary>
            /// Keeps track of whether the given part has been filtered previously or not.
            /// </summary>
            ConcurrentDictionary<ComposablePartDefinition, bool> filter =
                new ConcurrentDictionary<ComposablePartDefinition, bool>();

            /// <summary>
            /// Filters out non-generated parts.
            /// </summary>
            /// <param name="definition"></param>
            /// <returns></returns>
            public bool Filter(ComposablePartDefinition definition)
            {
                Contract.Requires<NullReferenceException>(definition != null);

                return filter.GetOrAdd(definition, _ =>
                {
                    var type = ReflectionModelServices.GetPartType(definition).Value;
                    return type != null && NancyReflectionContext.IsExportablePart(type);
                });
            }

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        protected NancyCatalog(ComposablePartCatalog source)
            : base(source, new FilterCache().Filter)
        {
            Contract.Requires<NullReferenceException>(source != null);
        }

    }

}
