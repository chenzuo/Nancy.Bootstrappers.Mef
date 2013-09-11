using System.ComponentModel.Composition.Primitives;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public class NancyTypeCatalogTests : NancyCatalogTests
    {

        protected override ComposablePartCatalog CreateCatalog()
        {
            return new NancyTypeCatalog(typeof(Nancy.NancyEngine).Assembly.GetTypes());
        }

    }

}
