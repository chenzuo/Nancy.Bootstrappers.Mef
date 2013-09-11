using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public class TypeHelperTests
    {

        [TestMethod]
        public void ReferencesNancy_TrueFromInsideTest()
        {
            Assert.IsTrue(TypeHelper.ReferencesNancy(typeof(NancyEngine)));
        }

        [TestMethod]
        public void ReferencesNancy_TrueFromOutsideTest()
        {
            Assert.IsTrue(TypeHelper.ReferencesNancy(typeof(TypeHelperTests)));
        }

        [TestMethod]
        public void ReferencesNancy_TrueFromInterfaceTest()
        {
            Assert.IsTrue(TypeHelper.ReferencesNancy(typeof(Tuple<INancyEngine>)));
        }

        [TestMethod]
        public void ReferencesNancy_FalseTest()
        {
            Assert.IsFalse(TypeHelper.ReferencesNancy(typeof(List<object>)));
        }

    }

}
