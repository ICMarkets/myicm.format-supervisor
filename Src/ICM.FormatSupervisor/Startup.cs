using Autofac;
using ICM.FormatSupervisor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

namespace ICM.FormatSupervisor
{
    public class Startup : StartupBase
    {
        public override string DisplayName => "FORMAT SUPERVISOR";

        public override void Configure(IApplicationBuilder app)
        {
            base.Configure(app);

            var schedulerService = app.ApplicationServices.GetService<SupervisorService>();
            var stopSignalSource = app.ApplicationServices.GetService<CancellationTokenSource>();

            ConnectKafka(schedulerService, stopSignalSource.Token);
        }

        public override void ConfigureServicesImpl(IServiceCollection services, ContainerBuilder builder)
        {
            base.ConfigureServicesImpl(services, builder);

            builder.RegisterType<SupervisorService>().AsSelf().SingleInstance();
        }

        private Task ConnectKafka(SupervisorService service, CancellationToken stopSignal)
        {
            return Task.Run(() => service.Start(stopSignal));
        }
    }
}
