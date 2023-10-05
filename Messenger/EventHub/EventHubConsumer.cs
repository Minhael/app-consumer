using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Common.Lang.Extensions;
using Common.Lang.Threading;
using Common.Telemetry;
using Common.Telemetry.OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Messenger.EventHub;

//  https://docs.microsoft.com/en-us/dotnet/api/overview/azure/messaging.eventhubs-readme
//  https://stackoverflow.com/questions/62214055/iasyncenumerable-yielding-from-event-handler
sealed class EventHubConsumer : IConsumer
{
    private static readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;
    private static readonly Tracer _tracer = Measure.CreateTracer<EventHubConsumer>();
    private static readonly Serilog.ILogger _logger = Log.ForContext<EventHubConsumer>();

    private readonly EventProcessorClient _client;
    private readonly IDictionary<string, string> _properties;

    public EventHubConsumer(EventProcessorClient client, IDictionary<string, string> properties)
    {
        _client = client;
        _properties = properties;
    }

    private Task InitHandler(PartitionInitializingEventArgs args)
    {
        args.DefaultStartingPosition = _properties.GetOrDefault(EventHubConsumerProps.AutoOffsetReset)?.ToLower() == "earliest" ? EventPosition.Earliest : EventPosition.Latest;
        return Task.CompletedTask;
    }

    private async Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.Error(args.Exception, "Failed to process message");
        await Task.CompletedTask;
    }

    public IAsyncDisposable Subscribe(Func<Message, CancellationToken, Task> subscription)
    {
        return DedicatedTask.Run(async token =>
        {
            async Task eventHandler(ProcessEventArgs args)
            {
                try
                {
                    var message = args.Convert();

                    var parentContext = DictionaryPropagator.Resume(message.Properties);
                    var attr = new SpanAttributes();
                    attr.Add("message.key", message.Key ?? "<Null>");
                    using var span = _tracer.StartActiveSpan($"eventhub.{_client.EventHubName} receive", SpanKind.Consumer, new SpanContext(parentContext.ActivityContext), attr);

                    _logger.Debug("Message received from {topic}: {message}", _client.EventHubName, message);
                    await subscription(message, args.CancellationToken);
                    await args.UpdateCheckpointAsync(args.CancellationToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.Warning("Processing of events of {topic} cancelled", _client.EventHubName);
                }
                catch (OperationCanceledException)
                {
                    _logger.Warning("Processing of events of {topic} cancelled", _client.EventHubName);
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Subscription processing failed. Exception suppressed. Handle it inside the subscription codes.");
                    await args.UpdateCheckpointAsync(args.CancellationToken);
                }
            }

            _client.PartitionInitializingAsync += InitHandler;
            _client.ProcessErrorAsync += ErrorHandler;
            _client.ProcessEventAsync += eventHandler;

            _logger.Debug("Subscribe to topic {topic}", _client.EventHubName);

            try
            {
                await _client.StartProcessingAsync(token);
                await Task.Delay(Timeout.Infinite, token);
            }
            catch (TaskCanceledException)
            {
                _logger.Warning("Event processor of topic {topic} cancelled", _client.EventHubName);
            }
            catch (OperationCanceledException)
            {
                _logger.Warning("Event processor of topic {topic} cancelled", _client.EventHubName);
            }
            finally
            {
                await _client.StopProcessingAsync(CancellationToken.None);
                _client.ProcessEventAsync -= eventHandler;
                _client.ProcessErrorAsync -= ErrorHandler;
                _client.PartitionInitializingAsync -= InitHandler;
            }
        });
    }
}