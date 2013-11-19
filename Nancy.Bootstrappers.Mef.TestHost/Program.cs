using System;

namespace Nancy.Bootstrappers.Mef.TestHost
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var host = new Host();

            Console.WriteLine("Nancy starting...");
            host.Start();
            Console.WriteLine("Nancy started.");

            Console.WriteLine("Press any key to stop Nancy.");
            Console.ReadLine();

            Console.WriteLine("Nancy stopping...");
            host.Stop();
            Console.WriteLine("Nancy stopped.");

            Console.ReadLine();
        }

    }

}
