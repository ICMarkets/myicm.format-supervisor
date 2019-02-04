using Confluent.Kafka;
using ICM.Common.Multithreading;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ICM.Common.Kafka
{
    public abstract class KafkaConsumer
    {
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        protected readonly ProducerConfig _producerConfig;
        protected ConsumerConfig _consumerConfig;
        protected Consumer<string, byte[]> _consumer;
        protected readonly MessageSerializer _messageSerializer;
        protected readonly WorkerCountdown _workerCountdown;

        public KafkaConsumer(string bootstrapServers, MessageSerializer messageSerializer, WorkerCountdown workerCountdown)
        {
            _producerConfig = new ProducerConfig { BootstrapServers = bootstrapServers };
            _messageSerializer = messageSerializer;
            _workerCountdown = workerCountdown;
        }

        public async Task Start(string[] topics, CancellationToken stopSignal)
        {
            try
            {
                _workerCountdown.AddCount();
                while (true)
                {
                    _consumer = new Consumer<string, byte[]>(_consumerConfig);

                    bool consuming = true;
                    // The client will automatically recover from non-fatal errors. You typically
                    // don't need to take any action unless an error is marked as fatal.
                    _consumer.OnError += (_, e) => consuming = !e.IsFatal;

                    _consumer.Subscribe(topics);
                    var topicsStr = string.Join(", ", topics);
                    Log.Log(LogLevel.Info, $"Starting Kafka consumer, listening to topics: {topicsStr}");
                    while (consuming)
                    {
                        try
                        {
                            var message = _consumer.Consume(stopSignal);
                            _workerCountdown.AddCount();
                            Task.Run(async () => await PerformTask(message)
                                .ContinueWith(task => _workerCountdown.Signal()));
                        }
                        catch (OperationCanceledException)
                        {
                            Log.Log(LogLevel.Info, $"Stopped Kafka consumer: {topicsStr}");
                            _consumer.Close();
                            return;
                        }
                        catch (ConsumeException e)
                        {
                            Log.Log(LogLevel.Warn, $"Kafka error occured: {e.Error.Reason}");
                        }
                        catch (Exception e)
                        {
                            Log.Log(LogLevel.Warn, $"Kafka error occured: {e.Message}");
                        }
                    }

                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    _consumer.Close();

                    Log.Log(LogLevel.Error, $"Fatal error, Kafka consumer stopped. Trying to create new consumer in 5 seconds...");
                    await Task.Delay(5000);
                }
            }
            finally
            {
                _workerCountdown.Signal();
            }
        }

        protected abstract Task PerformTask(ConsumeResult<string, byte[]> message);
    }
}
