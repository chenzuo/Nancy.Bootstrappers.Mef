using System.Reflection;

namespace Nancy.Bootstrappers.Mef.Composition.Registration
{

    /// <summary>
    /// Provides custom attributes for Nancy types that are not decorated with MEF attributes.
    /// </summary>
    /// <remarks>
    /// Really just serves as a small wrapper around <see cref="NancyRegistrationBuilder"/> so as to hide this
    /// implementation detail.
    /// </remarks>
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
