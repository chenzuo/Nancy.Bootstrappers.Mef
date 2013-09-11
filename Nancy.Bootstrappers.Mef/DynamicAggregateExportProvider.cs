using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace Nancy.Bootstrappers.Mef
{

    /// <summary>Retrieves exports provided by a collection of <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects.</summary>
    public class DynamicAggregateExportProvider : ExportProvider, IDisposable
    {

        int _disposed;
        ExportProviderCollection _providers;

        /// <summary>
        /// Collection of aggregated providers.
        /// </summary>
        public ExportProviderCollection Providers
        {
            get
            {
                ThrowIfDisposed();
                return _providers;
            }
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DynamicAggregateExportProvider()
        {
            _providers = new ExportProviderCollection();
            _providers.CollectionChanged += providers_CollectionChanged;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="providers"></param>
        public DynamicAggregateExportProvider(IEnumerable<ExportProvider> providers)
            : this()
        {
            if (providers != null)
                _providers.AddRange(providers);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="providers"></param>
        public DynamicAggregateExportProvider(params ExportProvider[] providers)
            : this(providers.Cast<ExportProvider>())
        {
            Contract.Requires<ArgumentNullException>(providers != null);
        }

        void providers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var oldItems = args.OldItems != null ? args.OldItems.Cast<ExportProvider>() : new ExportProvider[0];
            var newItems = args.NewItems != null ? args.NewItems.Cast<ExportProvider>() : new ExportProvider[0];

            foreach (var provider in oldItems)
            {
                provider.ExportsChanging -= provider_ExportsChanging;
                provider.ExportsChanged -= provider_ExportsChanged;
            }

            foreach (var provider in newItems)
            {
                provider.ExportsChanging += provider_ExportsChanging;
                provider.ExportsChanged += provider_ExportsChanged;
            }
        }

        void provider_ExportsChanging(object sender, ExportsChangeEventArgs args)
        {
            OnExportsChanging(args);
        }

        void provider_ExportsChanged(object sender, ExportsChangeEventArgs args)
        {
            OnExportsChanged(args);
        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            ThrowIfDisposed();

            foreach (var provider in _providers)
                foreach (var export in provider.GetExports(definition, atomicComposition))
                    yield return export;
        }

        [DebuggerStepThrough]
        void ThrowIfDisposed()
        {
            if (_disposed == 1)
                throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
                foreach (var provider in _providers)
                {
                    provider.ExportsChanging -= provider_ExportsChanging;
                    provider.ExportsChanged -= provider_ExportsChanged;
                }
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}
