using System.ComponentModel.Composition.Primitives;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public class NancyApplicationCatalogTests : NancyCatalogTests
    {

        protected override ComposablePartCatalog CreateCatalog()
        {
            return new NancyApplicationCatalog();
        }

    }

}
