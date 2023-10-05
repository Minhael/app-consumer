using Azure.Storage.Queues;
using Common.Serialization.Json;
using Common.Telemetry;
using Common.Telemetry.OpenTelemetry;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Azure;

public interface IQueue
{
    Task SendMessage(object contents, CancellationToken token = default);
    Task SendMessage(IEnumerable<object> contents, CancellationToken token = default);
    Task ReceiveMessages<T>(Func<T, CancellationToken, Task> subscription, int timeoutSeconds = 60, CancellationToken token = default) where T : class;
    Task ReceiveMessages<T>(int concurrency, Func<T, CancellationToken, Task> subscription, int timeoutSeconds = 60, CancellationToken token = default) where T : class;
}

class Queue : IQueue
{
    private static readonly Tracer _tracer = Measure.CreateTracer<Queue>();
    private static readonly Serilog.ILogger _logger = Log.ForContext<Queue>();

    private readonly string _connStr;
    private readonly string _queue;

    private readonly QueueClient _client;

    public Queue(string connStr, string queue)
    {
        _connStr = connStr;
        _queue = queue;
        _client = new(_connStr, _queue);
    }

    public async Task SendMessage(object contents, CancellationToken token = default)
    {
        var message = contents.ComposeJson();
        var attr = new SpanAttributes();
        attr.Add("queue.message", message);
        using var span = _tracer.StartActiveSpan($"{_queue} enqueue", SpanKind.Producer, initialAttributes: attr);
        await _client.SendMessageAsync(message, token);
    }

    public async Task SendMessage(IEnumerable<object> contents, CancellationToken token = default)
    {
        foreach (var content in contents.Select(x => x.ComposeJson()))
            await SendMessage(content, token);
    }

    public Task ReceiveMessages<T>(int concurrency, Func<T, CancellationToken, Task> subscription, int timeoutSeconds, CancellationToken token = default) where T : class
    {
        return Task.WhenAll(Enumerable.Range(0, concurrency).Select(x => Task.Run(() => ReceiveMessages<T>(subscription, timeoutSeconds, token))));
    }

    public async Task ReceiveMessages<T>(Func<T, CancellationToken, Task> subscription, int timeoutSeconds, CancellationToken token = default) where T : class
    {
        var client = new QueueClient(_connStr, _queue);

        while (!token.IsCancellationRequested)
        {
            try
            {
                var props = await client.GetPropertiesAsync(token);
                while (props.Value.ApproximateMessagesCount < 1)
                {
                    await Task.Delay(2000);
                    if (token.IsCancellationRequested)
                        return;
                    props = await client.GetPropertiesAsync(token);
                }

                var response = await client.ReceiveMessageAsync(TimeSpan.FromSeconds(timeoutSeconds), token);
                var contents = response.Value;

                // Receive the message at the same time with another instance 
                // One of them will get null contents
                if (contents == null)
                    continue;

                try
                {
                    var message = contents.MessageText.ParseJsonAs<T>();
                    _logger.Debug("Message received from {queue}: {message}", _queue, message);
                    var attr = new SpanAttributes();
                    attr.Add("queue.message", contents.MessageText);
                    using var span = _tracer.StartActiveSpan($"{_queue} dequeue", SpanKind.Consumer, new SpanContext(), initialAttributes: attr);

                    try
                    {
                        await subscription(message, token);
                        await client.DeleteMessageAsync(contents.MessageId, contents.PopReceipt);
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(e, "Application errors. Requeue message.");
                        await client.UpdateMessageAsync(contents.MessageId, contents.PopReceipt, null as string, TimeSpan.FromSeconds(60));
                        span?.SetStatus(Status.Error);
                        span?.RecordException(e);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to deserialize the message contents. Dropping the message");
                    await client.DeleteMessageAsync(contents.MessageId, contents.PopReceipt);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.Debug("Queue processor {topic} cancelled", _queue);
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("Queue processor {topic} cancelled", _queue);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected error");
            }
        }
    }
}