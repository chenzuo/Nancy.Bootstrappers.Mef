using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nancy.Bootstrappers.Mef.Composition.Registration;

namespace Nancy.Bootstrappers.Mef.Tests.Composition.Registration
{

    [TestClass]
    public class NancyRegistrationBuilderTests
    {

        public class TestStaticContentProvider : Nancy.IStaticContentProvider
        {

            public TestStaticContentProvider()
                : base()
            {

            }

            public Response GetContent(NancyContext context)
            {
                throw new NotImplementedException();
            }

        }

        static readonly NancyRegistrationBuilder context = new NancyRegistrationBuilder();
        static readonly Assembly assembly = ((ReflectionContext)context).MapAssembly(typeof(NancyEngine).Assembly);

        [TestMethod]
        public void ExportAttributeTest()
        {
            var a = typeof(Nancy.NancyEngine).GetCustomAttributes<ExportAttribute>().ToArray();
            Assert.IsTrue(a.Any(i => i.ContractType == typeof(NancyEngine)), "Could not resolve concrete type.");
            Assert.IsTrue(a.Any(i => i.ContractType == typeof(INancyEngine)), "Could not resolve expected interface.");
            Assert.IsTrue(a.Count() == 2);
        }

        [TestMethod]
        public void ConstructorAttributeTest()
        {
            var c = typeof(Nancy.NancyEngine).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var a = c[0].GetCustomAttribute<ImportingConstructorAttribute>();
            Assert.IsTrue(a != null, "Could not locate ImportingConstructorAttribute.");
        }

        [TestMethod]
        public void IsNancyPart()
        {
            Assert.IsFalse(NancyRegistrationBuilder.IsNancyPart(GetType()));
            Assert.IsTrue(NancyRegistrationBuilder.IsNancyPart(typeof(TestStaticContentProvider)), "TestStaticContentProvider should be a NancyPart.");
        }

    }

}
