using System.ComponentModel.Composition;

namespace Nancy.Bootstrappers.Mef.TestHost
{

    /// <summary>
    /// Simple test module.
    /// </summary>
    [Export(typeof(INancyModule))]
    public class TestModule :
        NancyModule
    {
        
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TestModule()
            : base("/test")
        {
            Get["/"] = x => "Test";
        }

    }

}
