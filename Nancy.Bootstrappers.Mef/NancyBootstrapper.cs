using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Mef.Composition.Hosting;
using Nancy.Bootstrappers.Mef.Extensions;
using Nancy.Diagnostics;

namespace Nancy.Bootstrappers.Mef
{

    /// <summary>
    /// Serves as a bootstrapper for Nancy when using the Managed Extensibility Framework.
    /// </summary>
    [InheritedExport(typeof(INancyBootstrapper))]
    [InheritedExport(typeof(INancyModuleCatalog))]
    public class NancyBootstrapper : NancyBootstrapperWithRequestContainerBase<CompositionContainer>
    {

        CompositionContainer parent;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public NancyBootstrapper()
            : base()
        {
            
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public NancyBootstrapper(CompositionContainer parent)
            : base()
        {
            Contract.Requires<ArgumentNullException>(parent != null);
            this.parent = parent;
        }

        #region Application Container

        /// <summary>
        /// Gets the <see cref="AggregateCatalog"/> which provides Nancy parts to the container.
        /// </summary>
        public AggregateCatalog ApplicationCatalog { get; private set; }

        /// <summary>
        /// Gets the <see cref="ExportProvider"/> which generates exports of Nancy parts for Nancy.
        /// </summary>
        public NancyExportProvider ApplicationExportProvider { get; private set; }

        /// <summary>
        /// Creates a new MEF container.
        /// </summary>
        /// <returns></returns>
        protected sealed override CompositionContainer CreateApplicationContainer()
        {
            return CreateApplicationContainer(null);
        }

        /// <summary>
        /// Override to implement custom creation of the application wide <see cref="ComposablePartCatalog"/>s. The
        /// default implementation of this method ensures that native Nancy parts are made available.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual AggregateCatalog CreateApplicationCatalogs(CompositionContainer parent)
        {
            Contract.Ensures(Contract.Result<AggregateCatalog>() != null);
            return new AggregateCatalog(new NancyAssemblyCatalog(typeof(NancyEngine).Assembly));
        }

        /// <summary>
        /// Override to implement custom creation of the application wide <see cref="NancyExportProvider"/>. The
        /// default implementation of this method returns a <see cref="NancyExportProvider"/> that exposes the parts
        /// configured in the catalog returned by the CreateApplicationCatalog method.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual NancyExportProvider CreateApplicationExportProvider(CompositionContainer parent)
        {
            Contract.Ensures(Contract.Result<NancyExportProvider>() != null);
            Contract.Ensures(ApplicationCatalog != null);
            return new NancyExportProvider(new CatalogExportProvider(ApplicationCatalog = CreateApplicationCatalog(parent)));
        }

        /// <summary>
        /// Override to implement custom creation of the application wide <see cref="CompositionContainer"/>. The
        /// default implementation of this method returns a new <see cref="CompositionContainer"/> which first searches
        /// for parts in the specified parent <see cref="CompositionContainer"/>, followed by parts returned by the
        /// <see cref="NancyExportProvider"/> created in the CreateApplicationExportProvider method.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected virtual CompositionContainer CreateApplicationContainer(CompositionContainer parent)
        {
            // default container resolves against an application catalog, passing through a custom ExportProvider
            var p1 = new CatalogExportProvider(ApplicationCatalog = CreateApplicationCatalog(parent));
            var p2 = new NancyExportProvider(p1);
            var cc = new CompositionContainer(
                CompositionOptions.DisableSilentRejection |
                CompositionOptions.ExportCompositionService |
                CompositionOptions.IsThreadSafe, parent, ApplicationExportProvider = p2);
            p1.SourceProvider = cc;

            return cc;
        }

        /// <summary>
        /// Provides a place to configure the newly created <see cref="CompositionContainer"/>. By default this method
        /// registers a <see cref="NancyAssemblyCatalog"/> associated with the Nancy assembly. Override this method to
        /// add additional part catalogs, or remove the default registration of the Nancy assembly.
        /// </summary>
        /// <param name="existingContainer"></param>
        protected sealed override void ConfigureApplicationContainer(CompositionContainer existingContainer)
        {
            existingContainer.Catalog.Add(new NancyAssemblyCatalog(typeof(NancyEngine).Assembly));
        }

        /// <summary>
        /// Bind the bootstrapper's implemented types into the container. This is necessary so a user can pass in a
        /// populated container but not have to take the responsibility of registering things like <see
        /// cref="INancyModuleCatalog"/> manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override sealed void RegisterBootstrapperTypes(CompositionContainer applicationContainer)
        {
            applicationContainer.ComposeParts(this);
        }

        /// <summary>
        /// Bind the default implementations of internally used types into the container as singletons.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override sealed void RegisterTypes(CompositionContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(typeRegistrations != null);

            //
            var types = typeRegistrations
                .Where(i => !ContainerHasExport(container, i.RegistrationType, i.ImplementationType))
                .SelectMany(i => new[] { i.ImplementationType, i.RegistrationType })
                .Select(i => i.UnderlyingSystemType)
                .Distinct()
                .OrderBy(i => i.FullName);
            if (types.Any())
                ((AggregateCatalog)container.Catalog).Catalogs.Add(new NancyTypeCatalog(types));
        }

        /// <summary>
        /// Bind the various collections into the container as singletons to later be resolved by IEnumerable{Type}
        /// constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
        protected override sealed void RegisterCollectionTypes(CompositionContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(collectionTypeRegistrations != null);

            RegisterTypes(container, collectionTypeRegistrations
                .SelectMany(i => i.ImplementationTypes.Select(j => new TypeRegistration(i.RegistrationType, j))));
        }

        /// <summary>
        /// Bind the given instances into the container.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(CompositionContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(instanceRegistrations != null);

            RegisterTypes(container, instanceRegistrations.Select(i => new TypeRegistration(i.RegistrationType, i.Implementation.GetType())));
            foreach (var r in instanceRegistrations)
                container.ComposeExportedValue(r.RegistrationType, r.Implementation);
        }

        #endregion

        #region Request Container

        /// <summary>
        /// Creates a per-request container. The default MEF implemenetation simply creates a new <see cref="Composition"/>
        /// container that uses the first one as a <see cref="ApplicationExportProvider"/> as well as an initial part catalog.
        /// </summary>
        /// <returns></returns>
        protected override CompositionContainer CreateRequestContainer()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return new CompositionContainer(
                new AggregateCatalog(ApplicationContainer.Catalog),
                CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService,
                ApplicationContainer);
        }

        /// <summary>
        /// Registers per-request modules in the per-request container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="moduleRegistrationTypes"></param>
        protected override void RegisterRequestContainerModules(CompositionContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(moduleRegistrationTypes != null);
        }

        #endregion

        /// <summary>
        /// Gets the diagnostics for initialization.
        /// </summary>
        protected override IDiagnostics GetDiagnostics()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValue<IDiagnostics>();
        }
            
        /// <summary>
        /// Gets all registered application startup tasks.
        /// </summary>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValues<IApplicationStartup>();
        }

        /// <summary>
        /// Gets all registered application registration tasks.
        /// </summary>
        protected override IEnumerable<IApplicationRegistrations> GetApplicationRegistrationTasks()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValues<IApplicationRegistrations>();
        }

        /// <summary>
        /// Gets the engine implementation from the container.
        /// </summary>
        protected override sealed INancyEngine GetEngineInternal()
        {
            Contract.Requires<ArgumentNullException>(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValueOrDefault<INancyEngine>();
        }

        /// <summary>
        /// Retrieve all module instances from the container.
        /// </summary>
        protected override sealed IEnumerable<INancyModule> GetAllModules(CompositionContainer container)
        {
            Contract.Requires<ArgumentNullException>(container != null);

            return container.GetExportedValues<INancyModule>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container.
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleType">Type of the module</param>
        protected override INancyModule GetModule(CompositionContainer container, Type moduleType)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(moduleType != null);

            return container.GetExports<INancyModule>()
                .Select(i => i.Value)
                .FirstOrDefault(i => i.GetType() == moduleType);
        }

    }

}
