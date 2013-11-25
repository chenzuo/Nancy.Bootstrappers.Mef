using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using Nancy.Hosting.Self;

namespace Nancy.Bootstrappers.Mef.TestHost
{

    public class Host
    {


        readonly Uri baseUri = new Uri("http://localhost:12333/nancy/");
        readonly CompositionContainer container;
        NancyHost nancy;

        public Host()
        {
            container = new CompositionContainer(
                new ApplicationCatalog(),
                CompositionOptions.DisableSilentRejection |
                CompositionOptions.IsThreadSafe |
                CompositionOptions.ExportCompositionService);

            // export initial values
            container.ComposeExportedValue(this);
        }

        public void Start()
        {
            // configure nancy
            nancy = new NancyHost(
                new NancyBootstrapper(container),
                new HostConfiguration()
                {
                    UrlReservations = new UrlReservations()
                    {
                        CreateAutomatically = true,
                    }
                },
                baseUri);
            nancy.Start();
        }

        /// <summary>
        /// Stops the service, from within synchronization context.
        /// </summary>
        public void Stop()
        {
            nancy.Stop();
            nancy.Dispose();
            nancy = null;
        }

    }

}
