using Confluent.Kafka;
using ICM.Common.Helpers;
using ICM.Common.Kafka;
using ICM.Common.Multithreading;
using ICM.FormatSupervisor.Enums;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ICM.FormatSupervisor.Services
{
    public class SupervisorService : KafkaConsumer
    {
        private readonly RuleService _ruleService;

        public SupervisorService(MessageSerializer serializer, WorkerCountdown workerCountdown, RuleService ruleService)
            : base(EnvironmentHelper.Variables[Variable.ICM_KAFKA], serializer, workerCountdown)
        {
            _consumerConfig = new ConsumerConfig
            {
                GroupId = EnvironmentHelper.Variables[Variable.ICM_SERVICENAME].ToLower(),
                BootstrapServers = EnvironmentHelper.Variables[Variable.ICM_KAFKA],
                AutoOffsetReset = AutoOffsetResetType.Earliest
            };
            _ruleService = ruleService;
        }

        public async Task Start(CancellationToken stopSignal)
        {
            await _ruleService.Load();
            var topics = _ruleService.GetTopics();
            await Start(topics, stopSignal);
        }

        public async Task<bool> PublishAdpRules(string topic)
        {
            var rules = await _ruleService.GetRulesFromDatabase(RuleType.ADP, true);
            Log.Log(LogLevel.Info, $"{rules.Count} ADP rules fetched from DB");
            try
            {
                using (var p = CreateProducer())
                {
                    foreach (var rule in rules)
                    {
                        await p.ProduceAsync(topic, new Message<string, byte[]>()
                        {
                            Key = rule.Key,
                            Value = _messageSerializer.Serialize(rule.Schema.ToString())
                        });
                    }
                }
                Log.Log(LogLevel.Info, $"{rules.Count} rules pushed to '{topic}' topic");
                return true;
            }
            catch (Exception ex)
            {
                Log.Log(LogLevel.Error, $"Error writing rules: {ex.Message}");
                return false;
            }
        }

        protected override async Task PerformTask(ConsumeResult<string, byte[]> message)
        {
            var messageText = _messageSerializer.Deserialize<string>(message.Value);
            var errors = _ruleService.Validate(message.Topic, message.Key, messageText);
            if (errors == null)
                return;

            using (var p = CreateProducer())
            {
                await p.ProduceAsync("format.issues", new Message<string, byte[]>() { Key = message.Topic, Value = _messageSerializer.Serialize(errors) });
                Log.Log(LogLevel.Warn, errors);
            }
        }
    }
}
