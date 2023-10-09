using System.Threading.Channels;
using Common.Lang.Threading;
using Common.Telemetry;
using Common.Telemetry.OpenTelemetry;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Messenger.Local;

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
            try
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
            }
            catch (TaskCanceledException)
            {
                _logger.Warning("Processing of events of {topic} cancelled", _topic);
            }
            catch (OperationCanceledException)
            {
                _logger.Warning("Processing of events of {topic} cancelled", _topic);
            }
        });
    }
}
