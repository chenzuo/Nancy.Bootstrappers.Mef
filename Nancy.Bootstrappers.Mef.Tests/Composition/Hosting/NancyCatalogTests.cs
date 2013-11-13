using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nancy.Bootstrappers.Mef.Composition.Hosting;
using Nancy.Routing;
using Nancy.ViewEngines;

namespace Nancy.Bootstrappers.Mef.Tests.Composition.Hosting
{

    [TestClass]
    public abstract class NancyCatalogTests
    {

        protected ComposablePartCatalog Catalog { get; private set; }

        /// <summary>
        /// Sample container.
        /// </summary>
        protected CompositionContainer Container { get; private set; }

        /// <summary>
        /// Gets the catalog we're testing against.
        /// </summary>
        /// <returns></returns>
        protected abstract ComposablePartCatalog CreateCatalog();

        [TestInitialize]
        public void TestInitialize()
        {
            var exp = new CatalogExportProvider(Catalog = CreateCatalog());
            var agg = new NancyExportProvider(exp);
            Container = new CompositionContainer(agg);
            exp.SourceProvider = agg;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Container.Dispose();
            Container = null;

            Catalog.Dispose();
            Catalog = null;
        }

        [TestMethod]
        public void QueryForSinglePart()
        {
            Assert.AreEqual(1, Catalog.Parts.Count(i => i.ExportDefinitions.Any(j => j.ContractName == "Nancy.INancyEngine")));
        }

        [TestMethod]
        public void GetExports_ViewLocationProvider()
        {
            Assert.IsNotNull(Container.GetExport<IViewLocationProvider>());
        }

        [TestMethod]
        public void GetExports_ViewLocator()
        {
            Assert.AreEqual(1, Container.GetExports<IViewLocator>().Count());
        }

        [TestMethod]
        public void GetExports_NancyEngine()
        {
            Assert.AreEqual(1, Container.GetExports<INancyEngine>().Count());
        }

        [TestMethod]
        public void GetExports_RequestDispatcher()
        {
            Assert.AreEqual(1, Container.GetExports<IRequestDispatcher>().Count());
        }

        [TestMethod]
        public void GetExports_RouteDescriptionProvider()
        {
            Assert.AreEqual(1, Container.GetExports<IRouteDescriptionProvider>().Count());
        }

        [TestMethod]
        public void GetExports_RouteResolver()
        {
            Assert.AreEqual(1, Container.GetExports<IRouteResolver>().Count());
        }

        [TestMethod]
        public void GetExports_RouteInvoker()
        {
            Assert.AreEqual(1, Container.GetExports<IRouteInvoker>().Count());
        }

        [TestMethod]
        public void GetExports_NancyModuleBuilder()
        {
            Assert.AreEqual(1, Container.GetExports<INancyModuleBuilder>().Count());
        }

        [TestMethod]
        public void GetExports_ViewFactory()
        {
            Assert.AreEqual(1, Container.GetExports<IViewFactory>().Count());
        }

        [TestMethod]
        public void GetExports_ViewResolver()
        {
            Assert.AreEqual(1, Container.GetExports<IViewResolver>().Count());
        }

    }

}
