using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CalendarHelper
{
    public class Program
    {

        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {


           var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()

                .Build();
            Console.WriteLine("Program Main Run");
            host.Run();
        }
    }
}
