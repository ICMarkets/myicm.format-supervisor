using Confluent.Kafka;
using ICM.Common.Helpers;
using ICM.Common.Kafka;
using ICM.Common.Multithreading;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace ICM.FormatSupervisor.Services
{
    public class SupervisorService : KafkaConsumer
    {
        public SupervisorService(MessageSerializer serializer, WorkerCountdown workerCountdown) 
            : base(EnvironmentHelper.Variables[Variable.ICM_KAFKA], serializer, workerCountdown)
        {
            _consumerConfig = new ConsumerConfig
            {
                GroupId = EnvironmentHelper.Variables[Variable.ICM_SERVICENAME].ToLower(),
                BootstrapServers = EnvironmentHelper.Variables[Variable.ICM_KAFKA],
                AutoOffsetReset = AutoOffsetResetType.Earliest
            };
        }

        public async Task Start(CancellationToken stopSignal)
        {
            // read topics
            await Start(null, stopSignal);
        }

        protected override async Task PerformTask(ConsumeResult<string, byte[]> message)
        {
        }
    }
}
