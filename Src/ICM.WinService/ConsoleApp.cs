using ICM.FormatSupervisor;
using System;
using System.Runtime.InteropServices;

namespace ICM.WinService
{
    public class ConsoleApp
    {
        private const int stopWaitMs = 5000;
        private Startup _startup = new Startup();

        public void Start()
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
            _startup.OnExit += clean =>
            {
                Environment.Exit(clean ? 0 : 1);
            };

            _startup.Start();
        }

        bool ConsoleEventCallback(int eventType)
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
