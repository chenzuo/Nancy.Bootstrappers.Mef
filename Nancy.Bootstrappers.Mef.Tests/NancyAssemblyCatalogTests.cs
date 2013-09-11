using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nancy.Routing;
using Nancy.ViewEngines;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public class NancyAssemblyCatalogTests : NancyCatalogTests
    {

        protected override ComposablePartCatalog CreateCatalog()
        {
            return new NancyAssemblyCatalog(typeof(Nancy.NancyEngine).Assembly);
        }

        [TestMethod]
        public void ViewLocationProviderExportTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<IViewLocationProvider>().ToArray();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void ViewLocatorExportTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<IViewLocator>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void EngineExportTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<INancyEngine>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void RequestDispatcherTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<IRequestDispatcher>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void RouteResolverTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<IRouteResolver>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void ModuleBuilderTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<INancyModuleBuilder>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void ViewFactoryTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<IViewFactory>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

        [TestMethod]
        public void ViewResolverTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<IViewResolver>();
            Assert.IsTrue(e != null);
            Assert.IsTrue(e.Count() == 1);
        }

    }

}
