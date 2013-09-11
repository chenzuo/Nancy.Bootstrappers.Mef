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

    }

}
