using Common.Lang.Threading;
using Confluent.Kafka;
using Serilog;

namespace Common.Messenger.Kafka;

public class KafkaConsumer : IConsumer
{
    private static readonly Serilog.ILogger _logger = Log.ForContext<KafkaConsumer>();

    private readonly string _topic;
    private readonly ConsumerBuilder<string, string> _consumer;

    public KafkaConsumer(string topic, ConsumerBuilder<string, string> consumer)
    {
        _topic = topic;
        _consumer = consumer;
    }

    public IAsyncDisposable Subscribe(Func<Message, CancellationToken, Task> subscription)
    {
        return DedicatedTask.Run(async token =>
        {
            using var consumer = _consumer.Build();
            consumer.Subscribe(_topic);
            try
            {
                while (true)
                {
                    var result = consumer.Consume(token);
                    try
                    {
                        await subscription(result.Convert(), token);
                        consumer.Commit(result);
                    }
                    catch (TaskCanceledException e)
                    {
                        throw e;
                    }
                    catch (OperationCanceledException e)
                    {
                        throw e;
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(e, "Subscription processing failed. Exception suppressed. Handle it inside the subscription codes.");
                        consumer.Commit(result);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                _logger.Warning("Event processor of topic {topic} cancelled", _topic);
            }
            catch (OperationCanceledException)
            {
                _logger.Warning("Event processor of topic {topic} cancelled", _topic);
            }
            finally
            {
                consumer.Close();
            }
        });
    }
}