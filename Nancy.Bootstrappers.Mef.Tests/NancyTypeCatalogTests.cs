using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public class NancyTypeCatalogTests : NancyCatalogTests
    {

        protected override NancyCatalog CreateCatalog()
        {
            return new NancyTypeCatalog(typeof(Nancy.NancyEngine).Assembly.GetTypes());
        }

    }

}
