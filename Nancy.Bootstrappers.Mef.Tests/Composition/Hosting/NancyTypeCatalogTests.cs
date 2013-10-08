using System.ComponentModel.Composition.Primitives;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nancy.Bootstrappers.Mef.Composition.Hosting;

namespace Nancy.Bootstrappers.Mef.Tests.Composition.Hosting
{

    /// <summary>
    /// Serves as a base catalog test. Other tests can extend this and substitute their own catalog to have the tests
    /// run against them as well.
    /// </summary>
    [TestClass]
    public class NancyTypeCatalogTests : NancyCatalogTests
    {

        protected override ComposablePartCatalog CreateCatalog()
        {
            return new NancyTypeCatalog(typeof(Nancy.NancyEngine).Assembly.GetTypes());
        }

    }

}
