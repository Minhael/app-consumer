using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Common.Lang.Extensions;
using Common.Telemetry;
using Common.Telemetry.OpenTelemetry;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Messenger.EventHub;

public sealed class EventHubProducer : IProducer
{
    private static readonly Tracer _tracer = Measure.CreateTracer<EventHubProducer>();
    private static readonly Serilog.ILogger _logger = Log.ForContext<EventHubProducer>();

    private readonly EventHubProducerClient _client;

    public EventHubProducer(EventHubProducerClient client)
    {
        _client = client;
    }

    public async Task Publish(Message message, CancellationToken token = default)
    {
        var payload = new EventData(message.Payload);
        payload.Properties.Put(message.Properties);

        var attr = new SpanAttributes();
        attr.Add("message.key", message.Key ?? "<Null>");
        using var span = _tracer.StartActiveSpan($"eventhub.{_client.EventHubName} publish", SpanKind.Producer, initialAttributes: attr);
        DictionaryPropagator.Prepare(message.Properties);

        await _client.SendAsync(payload.AsList(), new SendEventOptions { PartitionKey = message.Key }, token);
    }

    private void InjectTraceContextIntoBasicProperties(Message message, string key, string value)
    {
        try
        {
            message.Properties[key] = value;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to inject trace context.");
        }
    }
}