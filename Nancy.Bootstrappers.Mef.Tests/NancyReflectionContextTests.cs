using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nancy.Bootstrappers.Mef.Tests
{

    [TestClass]
    public class NancyReflectionContextTests
    {

        public class NancyModuleType : Nancy.Culture.DefaultCultureService
        {

            public NancyModuleType()
                :base(null)
            {

            }

        }

        static readonly NancyReflectionContext context = new NancyReflectionContext();
        static readonly Assembly assembly = context.MapAssembly(typeof(NancyEngine).Assembly);

        [TestMethod]
        public void AddGeneratedPartAttributeTest()
        {
            var t = assembly.GetType("Nancy.NancyEngine");
            var a = t.GetCustomAttributes<NancyGeneratedPartAttribute>().ToArray();
            Assert.IsTrue(a.Count() == 1);
        }

        [TestMethod]
        public void AddExportAttributesTest()
        {
            var t = assembly.GetType("Nancy.NancyEngine");
            var a = t.GetCustomAttributes<NancyExportAttribute>().ToArray();
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
        public void IsNancyTypeTest()
        {
            Assert.IsFalse(NancyReflectionContext.IsNancyType(GetType()));
            Assert.IsTrue(NancyReflectionContext.IsNancyType(typeof(NancyModuleType)));
        }

        [TestMethod]
        public void GetImportManyAttributeTest()
        {
            Assert.IsInstanceOfType(NancyReflectionContext.GetImportAttribute(typeof(IEnumerable<object>)), typeof(ImportManyAttribute));
            Assert.IsInstanceOfType(NancyReflectionContext.GetImportAttribute(typeof(ICollection<object>)), typeof(ImportManyAttribute));
            Assert.IsInstanceOfType(NancyReflectionContext.GetImportAttribute(typeof(IEnumerable)), typeof(ImportManyAttribute));
            Assert.IsInstanceOfType(NancyReflectionContext.GetImportAttribute(typeof(ICollection)), typeof(ImportManyAttribute));
            Assert.IsInstanceOfType(NancyReflectionContext.GetImportAttribute(typeof(object[])), typeof(ImportManyAttribute));
        }

        [TestMethod]
        public void GetImportAttributeTest()
        {
            Assert.IsInstanceOfType(NancyReflectionContext.GetImportAttribute(typeof(object)), typeof(ImportAttribute));
            Assert.IsInstanceOfType(NancyReflectionContext.GetImportAttribute(typeof(Lazy<object, object>)), typeof(ImportAttribute));
            Assert.IsInstanceOfType(NancyReflectionContext.GetImportAttribute(typeof(Func<bool, bool>)), typeof(ImportAttribute));
        }

    }

}
