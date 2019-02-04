using Confluent.Kafka;
using ICM.Common.Kafka;
using ICM.Common.Logging;
using NLog.Common;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NLog.Targets
{
    [Target("Kafka")]
    public class Kafka : TargetWithLayout 
    {
        private ProducerConfig _producerConfig;
        private readonly MessageSerializer _serializer = new MessageSerializer();
        private Producer<string, string> _producer;

        public Kafka()
        {
            brokers = new List<KafkaBroker>();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var message = Layout.Render(logEvent);
            SendMessageToQueue(logEvent, message);
            base.Write(logEvent);
        }

        private void SendMessageToQueue(LogEventInfo logEvent, string message)
        {
            try
            {
                if (_producerConfig == null)
                {
                    _producerConfig = new ProducerConfig { BootstrapServers = string.Join(",", brokers.Select(i => i.address)) };
                    _producer = new Producer<string, string>(_producerConfig);
                }
                _producer.BeginProduce(topic, new Message<string, string>() { Key = logEvent.Level.Name, Value = message });
            }
            catch (Exception ex)
            {
                InternalLogger.Error("Unable to send message to Kafka queue", ex);
            }
        }

        protected override void CloseTarget()
        {
            _producer?.Dispose();
            base.CloseTarget();
        }

        
        [RequiredParameter]
        public string topic { get; set; }

        [RequiredParameter]
        [ArrayParameter(typeof(KafkaBroker), "broker")]
        public IList<KafkaBroker> brokers { get; set; }

    }
}
