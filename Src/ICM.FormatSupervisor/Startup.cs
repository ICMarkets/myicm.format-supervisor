using Autofac;
using ICM.FormatSupervisor.Services;
using System;
using System.Threading;

namespace ICM.FormatSupervisor
{
    public class Startup : StartupBase
    {
        public override string DisplayName => "FORMAT SUPERVISOR";

        public void Start()
        {
            var provider = ConfigureServices();

            var schedulerService = ApplicationContainer.Resolve<SupervisorService>();
            var stopSignalSource = ApplicationContainer.Resolve<CancellationTokenSource>();

            ConnectKafka(schedulerService, stopSignalSource);
        }

        public override void ConfigureServicesImpl(ContainerBuilder builder)
        {
            base.ConfigureServicesImpl(builder);

            builder.RegisterType<SupervisorService>().AsSelf().SingleInstance();
            builder.RegisterType<RuleService>().AsSelf().SingleInstance();
        }

        private void ConnectKafka(SupervisorService service, CancellationTokenSource stopSignal)
        {
            service.Start(stopSignal.Token).Wait();
        }

        private Exception GetFirstIfAggregate(Exception ex)
        {
            return (ex is AggregateException) ? ((AggregateException)ex).InnerExceptions[0] : ex;
        }
    }
}
