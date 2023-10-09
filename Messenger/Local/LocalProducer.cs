using System.Threading.Channels;
using Common.Lang.Extensions;
using Common.Telemetry;
using Common.Telemetry.OpenTelemetry;
using NodaTime;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Messenger.Local;

class LocalProducer : IProducer
{
    private static readonly Tracer _tracer = Measure.CreateTracer<LocalProducer>();
    private static readonly Serilog.ILogger _logger = Log.ForContext<LocalProducer>();

    private readonly string _topic;
    private readonly Channel<Message> _channel;

    public LocalProducer(string topic, Channel<Message> channel)
    {
        _topic = topic;
        _channel = channel;
    }

    public async Task Publish(Message message, CancellationToken token = default)
    {
        var attr = new SpanAttributes();
        attr.Add("message.key", message.Key ?? "<Null>");
        using var span = _tracer.StartActiveSpan($"local.{_topic} publish", SpanKind.Producer, initialAttributes: attr);
        DictionaryPropagator.Prepare(message.Properties);
        await _channel.Writer.WriteNextAsync(message with { Utc = SystemClock.Instance.GetCurrentInstant().ToUnixTimeMilliseconds() }, token);
    }
}