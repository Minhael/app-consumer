using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Common.Lang.Extensions;
using Nito.AsyncEx.Synchronous;
using System.Collections.Immutable;

namespace Common.Messenger.EventHub;

public sealed class HubMessenger : IMessenger
{
    public static HubMessenger Build(string blobConnectionString, string blobContainerName, string connectionString, string[] topics, IDictionary<string, string>? properties = null)
    {
        var producers = topics.Select(x => new EventHubProducerClient(connectionString, x.ToLower())).ToDictionary(x => x.EventHubName.ToLower());
        var blobClient = new BlobContainerClient(blobConnectionString, blobContainerName);
        var processors = topics.Select(x => new EventProcessorClient(
            blobClient,
            properties?.GetOrDefault(EventHubConsumerProps.GroupId) ?? EventHubConsumerClient.DefaultConsumerGroupName,
            connectionString,
            x
        )).ToDictionary(x => x.EventHubName.ToLower());
        return new HubMessenger(producers, processors, topics, properties);
    }

    private readonly IReadOnlyDictionary<string, EventHubProducerClient> _producers;
    private readonly IReadOnlyDictionary<string, EventProcessorClient> _consumers;
    public string[] Topics { get; init; }
    private readonly IDictionary<string, string> _properties;

    public HubMessenger(IReadOnlyDictionary<string, EventHubProducerClient> producers, IReadOnlyDictionary<string, EventProcessorClient> consumers, string[] topics, IDictionary<string, string>? properties = null)
    {
        _producers = producers;
        _consumers = consumers;
        Topics = topics;
        _properties = properties ?? ImmutableDictionary<string, string>.Empty;
    }

    public IConsumer Consume(string topic, IDictionary<string, string>? properties = null)
    {
        return new EventHubConsumer(_consumers[topic.ToLower()], _properties.Merge(properties));
    }

    public IProducer Produce(string topic, IDictionary<string, string>? properties = null)
    {
        return new EventHubProducer(_producers[topic.ToLower()]);
    }

    //  https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#implement-both-dispose-and-async-dispose-patterns
    public void Dispose()
    {
        Dispose(disposing: true);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            Task.WhenAll(_producers.Values.Select(x => x.DisposeAsync().AsTask())).WaitAndUnwrapException();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
    }

    private async ValueTask DisposeAsyncCore()
    {
        await Task.WhenAll(_producers.Values.Select(x => x.DisposeAsync().AsTask())).ConfigureAwait(false);
    }
}