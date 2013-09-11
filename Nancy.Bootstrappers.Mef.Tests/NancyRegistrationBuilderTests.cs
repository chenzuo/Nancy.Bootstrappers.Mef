using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Reflection.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public class NancyRegistrationBuilderTests
    {

        public class NancyModuleType : Nancy.Culture.DefaultCultureService
        {

            public NancyModuleType()
                :base(null)
            {

            }

        }

        static readonly NancyRegistrationBuilder context = new NancyRegistrationBuilder();
        static readonly Assembly assembly = ((ReflectionContext)context).MapAssembly(typeof(NancyEngine).Assembly);

        [TestMethod]
        public void AddExportAttributesTest()
        {
            var t = assembly.GetType("Nancy.NancyEngine");
            var a = t.GetCustomAttributes<ExportAttribute>().ToArray();
            Assert.IsTrue(a.Any(i => i.ContractType == typeof(NancyEngine)));
            Assert.IsTrue(a.Any(i => i.ContractType == typeof(INancyEngine)));
            Assert.IsTrue(a.Count() == 2);
        }

        [TestMethod]
        public void AddConstructorAttributeTest()
        {
            var t = assembly.GetType("Nancy.NancyEngine");
            var c = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var a = c[0].GetCustomAttribute<ImportingConstructorAttribute>();
            Assert.IsTrue(a != null);
        }

        [TestMethod]
        public void IsExportablePartTest()
        {
            Assert.IsFalse(NancyRegistrationBuilder.IsExportablePart(GetType()));
            Assert.IsTrue(NancyRegistrationBuilder.IsExportablePart(typeof(NancyModuleType)));
        }

    }

}
