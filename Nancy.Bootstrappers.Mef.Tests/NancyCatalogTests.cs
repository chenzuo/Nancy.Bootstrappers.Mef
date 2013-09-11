using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public abstract class NancyCatalogTests
    {

        protected abstract ComposablePartCatalog CreateCatalog();

        [TestMethod]
        public void PartQueryTest()
        {
            var c = CreateCatalog();
            var p = c.Parts.Where(i => i.ExportDefinitions.Any(j => j.ContractName == "Nancy.INancyEngine"));
            Assert.IsTrue(p.Any());
        }

        [TestMethod]
        public void ExportTest()
        {
            var c = new CompositionContainer(CreateCatalog());
            var e = c.GetExports<INancyEngine>();
            Assert.IsTrue(e.Any());
        }

    }

}
