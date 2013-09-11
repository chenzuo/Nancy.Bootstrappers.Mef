using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Nancy.Bootstrappers.Mef
{

    /// <summary>
    /// Assembly catalog that provides export of Nancy implementations that are not decorated with standard
    /// MEF attributes. The <see cref="NancyBootstrapperContext"/> is used to virtualize MEF attributes.
    /// </summary>
    public class NancyAssemblyCatalog : AssemblyCatalog
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="assembly"></param>
        public NancyAssemblyCatalog(Assembly assembly)
            : base(assembly, new NancyReflectionContext())
        {
            Contract.Requires<NullReferenceException>(assembly != null);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="codeBase"></param>
        public NancyAssemblyCatalog(string codeBase)
            : base(codeBase, new NancyReflectionContext())
        {
            Contract.Requires<NullReferenceException>(codeBase != null);
        }

    }

}
