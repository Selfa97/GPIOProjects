using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.IO;
using System.Reflection;
#if DEBUG
using System.Diagnostics;
using System.Threading;
#endif

namespace GPIOWebRunner
{
    public class Program
    {
        private const int _port = 1223;

        public static void Main(string[] args)
        {
#if DEBUG
            do
            {
                Console.WriteLine("Waiting for Debugger to attach...");
                Thread.Sleep(5000);
            } while (!Debugger.IsAttached);
#endif

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseContentRoot(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                    webBuilder.UseKestrel();
                    webBuilder.UseUrls($"http://*:{_port}");
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .UseNLog();
    }
}
