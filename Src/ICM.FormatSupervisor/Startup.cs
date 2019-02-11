using Autofac;
using ICM.FormatSupervisor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
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

            ConnectKafka(schedulerService, stopSignalSource);
        }

        public override void ConfigureServicesImpl(IServiceCollection services, ContainerBuilder builder)
        {
            base.ConfigureServicesImpl(services, builder);

            builder.RegisterType<SupervisorService>().AsSelf().SingleInstance();
            builder.RegisterType<RuleService>().AsSelf().SingleInstance();
        }

        private Task ConnectKafka(SupervisorService service, CancellationTokenSource stopSignal)
        {
            return Task.Run(async () => await service.Start(stopSignal.Token))
                .ContinueWith(task => {
                    Log.Log(LogLevel.Error, $"Kafka thread exception: {GetFirstIfAggregate(task.Exception).Message}");
                    WaitExit(0);
                }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private Exception GetFirstIfAggregate(Exception ex)
        {
            return (ex is AggregateException) ? ((AggregateException)ex).InnerExceptions[0] : ex;
        }
    }
}
