using ICM.FormatSupervisor;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.InteropServices;

namespace ICM.WinService
{
    internal class ConsoleApp
    {
        private const int stopWaitMs = 150000;
        private readonly IWebHost _host;
        private readonly Startup _startup;

        public ConsoleApp(IWebHost host, Startup startup)
        {
            _host = host;
            _startup = startup;
        }

        protected void Run()
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            _host.Run();
        }

        private bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2 || eventType == 0) // on exit
            {
                _startup.WaitExit(stopWaitMs);
            }
            return false;
        }

        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }
}
