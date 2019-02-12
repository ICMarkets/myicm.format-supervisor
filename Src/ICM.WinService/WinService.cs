using ICM.FormatSupervisor;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace ICM.WinService
{
    public class WinService : ServiceBase
    {
        private const int stopWaitMs = 150000;
        private Startup _startup = new Startup();

        protected override void OnStart(string[] args)
        {
            Task.Run(() => _startup.Start());
        }

        protected override void OnStop()
        {
            RequestAdditionalTime(stopWaitMs);
            _startup.WaitExit(stopWaitMs);
        }
    }
}
