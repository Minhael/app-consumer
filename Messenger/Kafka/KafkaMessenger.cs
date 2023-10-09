using Confluent.Kafka;
using Nito.Disposables;

namespace Common.Messenger.Kafka;

// https://docs.confluent.io/platform/current/clients/confluent-kafka-dotnet/_site/api/Confluent.Kafka.html
public sealed class KafkaMessenger : IMessenger
{
    private IProducer<string, string> Producer { get; init; }
    private ConsumerConfig Consumer { get; init; }
    public string[] Topics { get; init; }

    public KafkaMessenger(IProducer<string, string> producer, ConsumerConfig consumer, string[] topics)
    {
        Producer = producer;
        Consumer = consumer;
        Topics = topics;
    }

    public IProducer Produce(string topic, IDictionary<string, string>? properties = null) => new KafkaProducer(topic, Producer);

    public IConsumer Consume(string topic, IDictionary<string, string>? properties = null)
    {
        return new KafkaConsumer(
            topic,
            new ConsumerBuilder<string, string>(Consumer)
                .SetLogHandler(Kafkas.LogHandler)
                .SetErrorHandler(Kafkas.ErrorHandler)
        );
    }

    public void Dispose()
    {
        Producer.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return Producer.ToAsyncDisposable().DisposeAsync();
    }
}