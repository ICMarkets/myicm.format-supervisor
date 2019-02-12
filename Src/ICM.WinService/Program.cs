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
            try
            {
                if (IsDebug(args))
                {
                    var console = new ConsoleApp();
                    console.Start();
                }
                else
                {
                    var service = new WinService();
                    ServiceBase.Run(service);
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
                    string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    File.WriteAllText(Path.Combine(assemblyPath, "error.log"), JsonConvert.SerializeObject(ex));
                }
            }
        }
    }
}
