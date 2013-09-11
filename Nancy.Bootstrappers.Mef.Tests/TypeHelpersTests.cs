using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public class TypeHelpersTests
    {

        [TestMethod]
        public void ReferencesNancy_TrueFromInsideTest()
        {
            Assert.IsTrue(TypeHelpers.ReferencesNancy(typeof(NancyEngine)));
        }

        [TestMethod]
        public void ReferencesNancy_TrueFromOutsideTest()
        {
            Assert.IsTrue(TypeHelpers.ReferencesNancy(typeof(TypeHelpersTests)));
        }

        [TestMethod]
        public void ReferencesNancy_TrueFromInterfaceTest()
        {
            Assert.IsTrue(TypeHelpers.ReferencesNancy(typeof(Tuple<INancyEngine>)));
        }

        [TestMethod]
        public void ReferencesNancy_FalseTest()
        {
            Assert.IsFalse(TypeHelpers.ReferencesNancy(typeof(List<object>)));
        }

    }

}
