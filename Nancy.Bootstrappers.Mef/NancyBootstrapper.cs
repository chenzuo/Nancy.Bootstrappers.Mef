using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    public class NancyBootstrapper :
        NancyBootstrapper<CompositionContainer>
    {

        readonly CompositionContainer container;
        readonly bool containerOwner;

        CompositionContainer nancyContainer;

        /// <summary>
        /// Initializes a new instance. Creates a container that scans for all assemblies in the current <see
        /// cref="AppDomain"/>.
        /// </summary>
        public NancyBootstrapper()
            : this(createDefaultContainer: true)
        {

        }

        /// <summary>
        /// Initializes a new instance. Optionally specifies whether a default container should be created. By default
        /// a container that scans for all assemblies in the current <see cref="AppDomain"/> is used.
        /// </summary>
        public NancyBootstrapper(bool createDefaultContainer = true)
            : base()
        {
            if (createDefaultContainer)
            {
                this.container = new CompositionContainer(new ApplicationCatalog());
                this.containerOwner = true;
            }
        }

        /// <summary>
        /// Initializes a new instance with a specified container.
        /// </summary>
        /// <param name="container"></param>
        public NancyBootstrapper(CompositionContainer container)
            : base()
        {
            Contract.Requires<ArgumentNullException>(container != null);

            this.container = container;
        }

        /// <summary>
        /// Creates a Nancy-specific <see cref="CompositionContainer"/> containing the <see
        /// cref="NancyExportProvider"/>. This container serves merely as a holder for the <see
        /// cref="NancyExportProvider"/> and should itself home no exports.
        /// </summary>
        /// <returns></returns>
        protected override CompositionContainer GetApplicationContainer()
        {
            // parent export providers
            var providers = new List<ExportProvider>()
            {
                new NancyExportProvider(),
            };

            // if we have a specified container, we use it as a source as well
            if (container != null)
                providers.Add(container);

            // default implementation
            return nancyContainer = new CompositionContainer(
                CompositionOptions.DisableSilentRejection |
                CompositionOptions.ExportCompositionService |
                CompositionOptions.IsThreadSafe,
                providers.ToArray());
        }

        /// <summary>
        /// Creates a per-request container. The default MEF implemenetation simply creates a new <see cref="Composition"/>
        /// container that uses the first one as a <see cref="ExportProvider"/> as well as an initial part catalog.
        /// </summary>
        /// <returns></returns>
        protected override CompositionContainer CreateRequestContainer()
        {
            Contract.Assert(ApplicationContainer != null);

            return new CompositionContainer(
                CompositionOptions.DisableSilentRejection |
                CompositionOptions.ExportCompositionService |
                CompositionOptions.IsThreadSafe,
                new NancyExportProvider(),
                ApplicationContainer);
        }

        public override void Dispose()
        {
            base.Dispose();

            // dispose of user provided, or default container
            if (container != null)
                if (containerOwner)
                    container.Dispose();

            // dispose of Nancy specific container
            if (nancyContainer != null)
                nancyContainer.Dispose();
        }

    }

    /// <summary>
    /// Serves as a bootstrapper for Nancy when using the Managed Extensibility Framework.
    /// </summary>
    [InheritedExport(typeof(INancyBootstrapper))]
    [InheritedExport(typeof(INancyModuleCatalog))]
    public abstract class NancyBootstrapper<TContainer> :
        NancyBootstrapperWithRequestContainerBase<TContainer>,
        IDisposable
        where TContainer : CompositionContainer, IDisposable
    {

        /// <summary>
        /// Initializes a new instance. Optionally specifies whether a default container should be created. By default
        /// a container that scans for all assemblies in the current <see cref="AppDomain"/> is used.
        /// </summary>
        public NancyBootstrapper()
            : base()
        {

        }

        #region Application Container

        /// <summary>
        /// Provides a place to configure the newly created <see cref="CompositionContainer"/>.
        /// </summary>
        /// <param name="container"></param>
        protected override void ConfigureApplicationContainer(TContainer container)
        {

        }

        /// <summary>
        /// Returns <c>true</c> if the type given by <paramref name="implementationType"/> is already available as an export
        /// of <paramref cref="contractType"/> in the container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="contractType"></param>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        bool IsTypeRegistered(CompositionContainer container, Type contractType, Type implementationType)
        {
            return container.GetExports(
                new ContractBasedImportDefinition(
                    AttributedModelServices.GetContractName(contractType),
                    AttributedModelServices.GetTypeIdentity(contractType),
                    null,
                    ImportCardinality.ZeroOrMore,
                    false,
                    false,
                    CreationPolicy.Any))
            .Select(i =>
                ReflectionModelServices.GetExportingMember(i.Definition))
            .Where(i =>
                // export must be associated with a Type, implemented by the specified type
                i.MemberType == MemberTypes.TypeInfo &&
                i.GetAccessors().Any(j => ((TypeInfo)j).UnderlyingSystemType == implementationType.UnderlyingSystemType))
            .Any();
        }

        /// <summary>
        /// Bind the bootstrapper's implemented types into the container. This is necessary so a user can pass in a
        /// populated container but not have to take the responsibility of registering things like <see
        /// cref="INancyModuleCatalog"/> manually.
        /// </summary>
        /// <param name="applicationContainer">Application container to register into</param>
        protected override sealed void RegisterBootstrapperTypes(TContainer applicationContainer)
        {
            applicationContainer.ComposeParts(this);
        }

        /// <summary>
        /// Adds the specified <see cref="ComposablePartCatalog"/> to the specified container. The default
        /// implementation expects the container to have a <see cref="NancyExportProvider"/> in it's Providers
        /// collection. To override this method, ensure that the catalog you add is added to a <see
        /// cref="NancyExportProvider"/>. This ensures behavior expected by Nancy, such as default instance resolution,
        /// is provided properly.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="catalog"></param>
        protected virtual void AddCatalog(TContainer container, ComposablePartCatalog catalog)
        {
            Contract.Requires<ArgumentNullException>(container != null);
            Contract.Requires<ArgumentNullException>(catalog != null);

            var provider = container.Providers.OfType<NancyExportProvider>().FirstOrDefault();
            if (provider == null)
                throw new NullReferenceException("Container does not contain a NancyExportProvider. Override RegisterTypes to provide custom implementation.");

            // add new catalog
            provider.Providers.Add(new CatalogExportProvider(catalog)
            {
                SourceProvider = container,
            });
        }

        /// <summary>
        /// Bind the default implementations of internally used types into the container as singletons.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="typeRegistrations">Type registrations to register</param>
        protected override void RegisterTypes(TContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            Contract.Assert(container != null);
            Contract.Assert(typeRegistrations != null);

            var types = typeRegistrations
                .Where(i => i.ImplementationType.IsClass)
                .Where(i => !IsTypeRegistered(container, i.RegistrationType, i.ImplementationType))
                .SelectMany(i => new[] { i.ImplementationType, i.RegistrationType })
                .Select(i => i.UnderlyingSystemType)
                .Distinct()
                .OrderBy(i => i.FullName);
            if (types.Any())
                AddCatalog(container, new NancyTypeCatalog(types));
        }

        /// <summary>
        /// Bind the various collections into the container as singletons to later be resolved by IEnumerable{Type}
        /// constructor dependencies.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="collectionTypeRegistrations">Collection type registrations to register</param>
        protected override sealed void RegisterCollectionTypes(TContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            Contract.Assert(container != null);
            Contract.Assert(collectionTypeRegistrations != null);

            // transform for RegisterTypes implementation
            RegisterTypes(container, collectionTypeRegistrations
                .SelectMany(i => i.ImplementationTypes
                    .Select(j => new TypeRegistration(i.RegistrationType, j))));
        }

        /// <summary>
        /// Bind the given instances into the container.
        /// </summary>
        /// <param name="container">Container to register into</param>
        /// <param name="instanceRegistrations">Instance registration types</param>
        protected override void RegisterInstances(TContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            Contract.Assert(container != null);
            Contract.Assert(instanceRegistrations != null);

            // register the types
            RegisterTypes(container, instanceRegistrations
                .Select(i => new TypeRegistration(i.RegistrationType, i.Implementation.GetType())));

            // and export the instances
            foreach (var r in instanceRegistrations)
                container.ComposeExportedValue(r.RegistrationType, r.Implementation);
        }

        #endregion

        #region Request Container

        /// <summary>
        /// Registers per-request modules in the per-request container.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="moduleRegistrationTypes"></param>
        protected override void RegisterRequestContainerModules(TContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            Contract.Assert(container != null);
            Contract.Assert(moduleRegistrationTypes != null);

            RegisterModules(container, moduleRegistrationTypes);
        }

        #endregion

        /// <summary>
        /// Gets the diagnostics for initialization.
        /// </summary>
        protected override IDiagnostics GetDiagnostics()
        {
            Contract.Assert(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValue<IDiagnostics>();
        }

        /// <summary>
        /// Gets all registered application startup tasks.
        /// </summary>
        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            Contract.Assert(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValues<IApplicationStartup>();
        }

        /// <summary>
        /// Gets all registered application registration tasks.
        /// </summary>
        protected override IEnumerable<IApplicationRegistrations> GetApplicationRegistrationTasks()
        {
            Contract.Assert(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValues<IApplicationRegistrations>();
        }

        /// <summary>
        /// Gets the engine implementation from the container.
        /// </summary>
        protected override sealed INancyEngine GetEngineInternal()
        {
            Contract.Assert(ApplicationContainer != null);

            return ApplicationContainer.GetExportedValueOrDefault<INancyEngine>();
        }

        /// <summary>
        /// Retrieve all module instances from the container.
        /// </summary>
        protected override sealed IEnumerable<INancyModule> GetAllModules(TContainer container)
        {
            Contract.Assert(container != null);

            return container.GetExportedValues<INancyModule>();
        }

        /// <summary>
        /// Retreive a specific module instance from the container.
        /// </summary>
        /// <param name="container">Container to use</param>
        /// <param name="moduleType">Type of the module</param>
        protected override INancyModule GetModule(TContainer container, Type moduleType)
        {
            Contract.Assert(container != null);
            Contract.Assert(moduleType != null);

            return container.GetExports<INancyModule>()
                .Select(i => i.Value)
                .FirstOrDefault(i => i.GetType() == moduleType);
        }

        public virtual void Dispose()
        {

        }

    }

}
