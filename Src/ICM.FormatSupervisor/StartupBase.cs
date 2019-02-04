using Autofac;
using Autofac.Extensions.DependencyInjection;
using ICM.Common.Helpers;
using ICM.Common.Kafka;
using ICM.Common.Multithreading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;
using System;
using System.Threading;

namespace ICM.FormatSupervisor
{
    public abstract class StartupBase : IStartup
    {
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        public IContainer ApplicationContainer { get; protected set; }
        public abstract string DisplayName { get; }
        private readonly WorkerCountdown _workerCountdown;
        private readonly CancellationTokenSource _stopSignalSource;

        public StartupBase()
        {
            _stopSignalSource = new CancellationTokenSource();
            _workerCountdown = new WorkerCountdown();
        }

        /// <summary>
        /// Must be called on service exit to ensure that all running tasks ran to completion
        /// </summary>
        /// <param name="stopWaitMs">Milliseconds to wait before forcefully abort</param>
        public void WaitExit(int stopWaitMs)
        {
            _stopSignalSource.Cancel();
            _workerCountdown.Signal(); // to remove last count (initial value is 1)

            try
            {
                _workerCountdown.Wait(stopWaitMs);
                Log.Log(LogLevel.Info, $"Exit clean");
            }
            catch (OperationCanceledException)
            {
                Log.Log(LogLevel.Error, $"Exit by {stopWaitMs} ms timeout (workers aborted: {_workerCountdown.CurrentCount})");
            }
        }

        ~StartupBase()
        {
            _workerCountdown.Dispose();
            _stopSignalSource.Dispose();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            EnvironmentHelper.Load();

            Log.Log(LogLevel.Info, $"-----------------   {DisplayName}   -----------------");
            Log.Log(LogLevel.Info, $"-----------------   IC MARKETS (c) {DateTime.UtcNow.Year}   -------------------------");
            Log.Log(LogLevel.Info, $"Service: {EnvironmentHelper.Variables[Variable.ICM_SERVICENAME]}; Instance ID: {EnvironmentHelper.Variables[Variable.HOSTNAME]}");
            Log.Log(LogLevel.Info, "");

            // autofac dependency
            var builder = new ContainerBuilder();

            ConfigureServicesImpl(services, builder);

            builder.RegisterType<MessageSerializer>().AsSelf().SingleInstance();
            builder.Register(context => _workerCountdown).SingleInstance().ExternallyOwned();
            builder.Register(context => _stopSignalSource).SingleInstance().ExternallyOwned();

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public virtual void ConfigureServicesImpl(IServiceCollection services, ContainerBuilder builder)
        {
            services.AddMvc();
            builder.Populate(services);
        }

        public virtual void Configure(IApplicationBuilder app)
        {
            app.UseMvcWithDefaultRoute();
            app.UseExceptionHandler(b => CatchExceptions(b));
        }

        protected virtual void CatchExceptions(IApplicationBuilder builder)
        {
            builder.Use(async (context, next) =>
            {
                var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;
                context.Response.ContentType = "application/json";
                string errorText = null;

                if (error?.Error != null)
                {
                    context.Response.StatusCode = 500;
                    errorText = JsonConvert.SerializeObject(new { global = new[] { error.Error.Message } });
                }
                else
                {
                    await next();
                }

                await context.Response.WriteAsync(errorText);
            });
        }
    }
}
