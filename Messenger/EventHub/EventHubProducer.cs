using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Common.Lang.Extensions;
using Common.Telemetry;
using Common.Telemetry.OpenTelemetry;
using OpenTelemetry.Trace;

namespace Common.Messenger.EventHub;

public sealed class EventHubProducer : IProducer
{
    private static readonly Tracer _tracer = Measure.CreateTracer<EventHubProducer>();
    private readonly EventHubProducerClient _client;

    public EventHubProducer(EventHubProducerClient client)
    {
        _client = client;
    }

    public async Task Publish(Message message, CancellationToken token = default)
    {
        var attr = new SpanAttributes();
        attr.Add("message.key", message.Key ?? "<Null>");
        using var span = _tracer.StartActiveSpan($"eventhub.{_client.EventHubName} publish", SpanKind.Producer, initialAttributes: attr);
        DictionaryPropagator.Prepare(message.Properties);

        var payload = new EventData(message.Payload);
        payload.Properties.Put(message.Properties);
        await _client.SendAsync(payload.AsList(), new SendEventOptions { PartitionKey = message.Key }, token);
    }
}