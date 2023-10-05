
using Common.Lang.Extensions;
using Common.Serialization.Json;
using Confluent.Kafka;
using Serilog;

namespace Common.Messenger.Kafka;

sealed class KafkaProducer : IProducer
{
    private static readonly Serilog.ILogger _logger = Log.ForContext<KafkaProducer>();

    private readonly string _topic;
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(string topic, IProducer<string, string> producer)
    {
        _topic = topic;
        _producer = producer;
    }

    public async Task Publish(Message message, CancellationToken token = default)
    {
        var payload = new Message<string, string> { Value = message.Payload, Headers = new Headers() };
        if (message.Key != null) payload.Key = message.Key;
        message.Properties?.ForEach(x => payload.Headers.Add(x.Key, x.Value.ComposeJsonBytes()));
        var result = await _producer.ProduceAsync(_topic.ToLower(), payload, token);
        _producer.Flush(token);
    }
}
