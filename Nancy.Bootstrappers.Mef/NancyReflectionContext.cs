using System.Reflection;

namespace Nancy.Bootstrappers.Mef
{

    /// <summary>
    /// Provides custom attributes for Nancy types that are not decorated with MEF attributes.
    /// </summary>
    public class NancyReflectionContext : ReflectionContext
    {

        NancyRegistrationBuilder builder = new NancyRegistrationBuilder();

        public override TypeInfo GetTypeForObject(object value)
        {
            return builder.GetTypeForObject(value);
        }

        public override Assembly MapAssembly(Assembly assembly)
        {
            return builder.MapAssembly(assembly);
        }

        public override TypeInfo MapType(TypeInfo type)
        {
            return builder.MapType(type);
        }

    }

}
