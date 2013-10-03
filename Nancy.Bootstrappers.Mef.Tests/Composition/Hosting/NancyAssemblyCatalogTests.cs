using System.ComponentModel.Composition.Primitives;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nancy.Bootstrappers.Mef.Composition.Hosting;

namespace Nancy.Bootstrappers.Mef.Tests.Composition.Hosting
{

    [TestClass]
    public class NancyAssemblyCatalogTests : NancyCatalogTests
    {

        protected override ComposablePartCatalog CreateCatalog()
        {
            return new NancyAssemblyCatalog(typeof(Nancy.NancyEngine).Assembly);
        }

    }

}
