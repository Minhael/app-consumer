using System.Collections.Concurrent;
using System.Threading.Channels;
using Common.Lang.Extensions;
using Common.Lang.Threading;
using Common.Telemetry;
using Common.Telemetry.OpenTelemetry;
using NodaTime;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Messenger;

//  https://github.com/open-telemetry/opentelemetry-dotnet/blob/f471a9f197d797015123fe95d3e12b6abf8e1f5f/examples/MicroserviceExample/Utils/Messaging/MessageReceiver.cs#L53
public sealed class LocalMessenger : IMessenger
{
    public string[] Topics { get; set; } = Array.Empty<string>();

    private readonly IDictionary<string, Channel<Message>> _channels = new ConcurrentDictionary<string, Channel<Message>>(StringComparer.OrdinalIgnoreCase);

    public IConsumer Consume(string topic, IDictionary<string, string>? properties = null)
    {
        return new LocalConsumer(topic, _channels[topic]);
    }

    public IProducer Produce(string topic, IDictionary<string, string>? properties = null)
    {
        return new LocalProducer(topic, _channels[topic]);
    }
    public void Dispose()
    {
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }
}

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
        await _channel.Writer.WriteNextAsync(message with { Timestamp = SystemClock.Instance.GetCurrentInstant() }, token);
    }
}

class LocalConsumer : IConsumer
{
    private static readonly Tracer _tracer = Measure.CreateTracer<LocalConsumer>();
    private static readonly Serilog.ILogger _logger = Log.ForContext<LocalConsumer>();

    private readonly string _topic;
    private readonly Channel<Message> _channel;

    public LocalConsumer(string topic, Channel<Message> channel)
    {
        _topic = topic;
        _channel = channel;
    }

    public IAsyncDisposable Subscribe(Func<Message, CancellationToken, Task> subscription)
    {
        return DedicatedTask.Run(async token =>
        {
            await foreach (var m in _channel.Reader.ReadAllAsync(token))
            {
                var parentContext = DictionaryPropagator.Resume(m.Properties);
                var attr = new SpanAttributes();
                attr.Add("message.key", m.Key ?? "<Null>");
                using var span = _tracer.StartActiveSpan($"local.{_topic} receive", SpanKind.Consumer, new SpanContext(parentContext.ActivityContext), attr);
                try
                {
                    await subscription(m, token);
                }
                catch (TaskCanceledException)
                {
                    _logger.Warning("Event processor of topic {topic} cancelled", _topic);
                }
                catch (OperationCanceledException)
                {
                    _logger.Warning("Event processor of topic {topic} cancelled", _topic);
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Subscription of {topic} processing failed. Exception suppressed. Message: {message}", _topic, m);
                    span?.SetStatus(Status.Error);
                    span?.RecordException(e);
                }
            }
        });
    }
}
