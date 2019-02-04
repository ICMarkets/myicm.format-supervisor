using ICM.FormatSupervisor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace ICM.WinService
{
    class Program
    {
        private static bool IsDebug(string[] args) => Debugger.IsAttached || args.Contains("-debug") || args.Contains("-d");

        static void Main(string[] args)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(Path.Combine(assemblyPath, "config.json"), false)
                    .Build();

                var startup = new Startup();

                var host = new WebHostBuilder()
                .UseKestrel()
                .UseConfiguration(config)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(startup);
                })
                .Build();

                if (IsDebug(args))
                {
                    var console = new ConsoleApp(host, startup);
                    host.Run();
                }
                else
                {
                    var webHostService = new WinService(host, startup);
                    ServiceBase.Run(webHostService);
                }
            }
            catch (Exception ex)
            {
                if (IsDebug(args))
                {
                    Console.WriteLine($"Error starting service");
                    Console.WriteLine(JsonConvert.SerializeObject(ex));
                    Console.ReadLine();
                }
                else
                {
                    File.WriteAllText(Path.Combine(assemblyPath, "error.log"), JsonConvert.SerializeObject(ex));
                }
            }
        }
    }
}
