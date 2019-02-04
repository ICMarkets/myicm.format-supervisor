using ICM.FormatSupervisor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using NLog;

namespace ICM.WinService
{
    internal class WinService : WebHostService
    {
        private const int stopWaitMs = 150000;
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly Startup _startup;

        public WinService(IWebHost host, Startup startup) : base(host)
        {
            _startup = startup;
        }

        protected override void OnStopping()
        {
            RequestAdditionalTime(stopWaitMs);
            _startup.WaitExit(stopWaitMs);
        }
    }
}
