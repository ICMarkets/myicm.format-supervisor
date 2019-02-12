using Autofac;
using Autofac.Extensions.DependencyInjection;
using ICM.Common.Helpers;
using ICM.Common.Kafka;
using ICM.Common.Multithreading;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Threading;

namespace ICM.FormatSupervisor
{
    public abstract class StartupBase
    {
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        public IContainer ApplicationContainer { get; protected set; }
        public abstract string DisplayName { get; }
        private readonly WorkerCountdown _workerCountdown;
        private readonly CancellationTokenSource _stopSignalSource;
        public event Action<bool> OnExit;

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
                OnExit?.Invoke(true);
            }
            catch (OperationCanceledException)
            {
                Log.Log(LogLevel.Error, $"Exit by {stopWaitMs} ms timeout (workers aborted: {_workerCountdown.CurrentCount})");
                OnExit?.Invoke(false);
            }
        }

        ~StartupBase()
        {
            _workerCountdown.Dispose();
            _stopSignalSource.Dispose();
        }

        public IServiceProvider ConfigureServices()
        {
            EnvironmentHelper.Load();

            Log.Log(LogLevel.Info, $"-----------------   {DisplayName}   -----------------");
            Log.Log(LogLevel.Info, $"-----------------   IC MARKETS (c) {DateTime.UtcNow.Year}   -------------------------");
            Log.Log(LogLevel.Info, $"Service: {EnvironmentHelper.Variables[Variable.ICM_SERVICENAME]}; Instance ID: {EnvironmentHelper.Variables[Variable.HOSTNAME]}");
            Log.Log(LogLevel.Info, "");

            // autofac dependency
            var builder = new ContainerBuilder();

            ConfigureServicesImpl(builder);

            builder.RegisterType<MessageSerializer>().AsSelf().SingleInstance();
            builder.Register(context => _workerCountdown).SingleInstance().ExternallyOwned();
            builder.Register(context => _stopSignalSource).SingleInstance().ExternallyOwned();

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public virtual void ConfigureServicesImpl(ContainerBuilder builder)
        {
        }
    }
}
