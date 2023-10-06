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

    public IAsyncDisposable Subscribe(Func<Message, CancellationToken, Task> subscription)
    {
        var ssp = new Subscription(_client.EventHubName, subscription, _properties);
        return DedicatedTask.Run(async token =>
        {
            _client.PartitionInitializingAsync += ssp.InitHandler;
            _client.ProcessErrorAsync += ssp.ErrorHandler;
            _client.ProcessEventAsync += ssp.EventHandler;

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
                _client.ProcessEventAsync -= ssp.EventHandler;
                _client.ProcessErrorAsync -= ssp.ErrorHandler;
                _client.PartitionInitializingAsync -= ssp.InitHandler;
            }
        });
    }

    sealed class Subscription
    {
        private readonly string _topic;
        private readonly Func<Message, CancellationToken, Task> _subscription;
        private readonly IDictionary<string, string> _properties;

        public Subscription(string topic, Func<Message, CancellationToken, Task> subscription, IDictionary<string, string> properties)
        {
            _topic = topic;
            _subscription = subscription;
            _properties = properties;
        }

        public Task InitHandler(PartitionInitializingEventArgs args)
        {
            args.DefaultStartingPosition = _properties.GetOrDefault(EventHubConsumerProps.AutoOffsetReset)?.ToLower() == "earliest" ? EventPosition.Earliest : EventPosition.Latest;
            return Task.CompletedTask;
        }

        public async Task EventHandler(ProcessEventArgs args)
        {
            try
            {
                var message = args.Convert();

                var parentContext = DictionaryPropagator.Resume(message.Properties);
                var attr = new SpanAttributes();
                attr.Add("message.key", message.Key ?? "<Null>");
                using var span = _tracer.StartActiveSpan($"eventhub.{_topic} receive", SpanKind.Consumer, new SpanContext(parentContext.ActivityContext), attr);

                _logger.Debug("Message received from {topic}: {message}", _topic, message);
                await _subscription(message, args.CancellationToken);
                await args.UpdateCheckpointAsync(args.CancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.Warning("Processing of events of {topic} cancelled", _topic);
            }
            catch (OperationCanceledException)
            {
                _logger.Warning("Processing of events of {topic} cancelled", _topic);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Subscription processing failed. Exception suppressed. Handle it inside the subscription codes.");
                await args.UpdateCheckpointAsync(args.CancellationToken);
            }
        }

        public async Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.Error(args.Exception, "Failed to process message");
            await Task.CompletedTask;
        }
    }
}