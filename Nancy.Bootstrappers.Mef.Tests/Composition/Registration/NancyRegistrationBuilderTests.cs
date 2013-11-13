using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Nancy.Bootstrappers.Mef.Composition.Registration;

namespace Nancy.Bootstrappers.Mef.Tests.Composition.Registration
{

    public class NancyRegistrationBuilderTests_TestStaticContentProvider : Nancy.IStaticContentProvider
    {

        public NancyRegistrationBuilderTests_TestStaticContentProvider()
            : base()
        {

        }

        public Response GetContent(NancyContext context)
        {
            throw new NotImplementedException();
        }

    }

    [TestClass]
    public class NancyRegistrationBuilderTests
    {

        static readonly NancyRegistrationBuilder context = new NancyRegistrationBuilder();
        static readonly Assembly assembly = ((ReflectionContext)context).MapAssembly(typeof(NancyEngine).Assembly);

        [TestMethod]
        public void ExportAttributeTest()
        {
            var a = context.MapType(typeof(Nancy.NancyEngine).GetTypeInfo())
                .GetCustomAttributes<ExportAttribute>().ToArray();
            Assert.IsTrue(a.Any(i => i.ContractType == null), "Could not resolve concrete type.");
            Assert.IsTrue(a.Any(i => i.ContractType == typeof(INancyEngine)), "Could not resolve expected interface.");
            Assert.IsTrue(a.Count() == 2);
        }

        [TestMethod]
        public void ConstructorAttributeTest()
        {
            var c = context.MapType(typeof(Nancy.NancyEngine).GetTypeInfo())
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var a = c[0].GetCustomAttribute<ImportingConstructorAttribute>();
            Assert.IsTrue(a != null, "Could not locate ImportingConstructorAttribute.");
        }

        [TestMethod]
        public void IsExportableNancyPart()
        {
            Assert.IsFalse(NancyRegistrationBuilder.IsExportableNancyPart(GetType()));
            Assert.IsTrue(NancyRegistrationBuilder.IsExportableNancyPart(typeof(NancyRegistrationBuilderTests_TestStaticContentProvider)), "TestStaticContentProvider should be a NancyPart.");
        }

    }

}
