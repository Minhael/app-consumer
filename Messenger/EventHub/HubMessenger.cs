using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Common.Lang.Extensions;
using Nito.AsyncEx.Synchronous;

namespace Common.Messenger.EventHub;

public sealed class HubMessenger : IMessenger
{
    private readonly IReadOnlyDictionary<string, EventHubProducerClient> _producers;
    public string[] Topics { get; init; }
    private readonly BlobContainerClient _blobContainerClient;
    private readonly string _connectionString;
    private readonly IDictionary<string, string> _properties;

    public HubMessenger(IReadOnlyDictionary<string, EventHubProducerClient> producers, string[] topics, BlobContainerClient blobContainerClient, string connectionString, IDictionary<string, string> properties)
    {
        _producers = producers;
        Topics = topics;
        _blobContainerClient = blobContainerClient;
        _connectionString = connectionString;
        _properties = properties;
    }

    public IConsumer Consume(string topic, IDictionary<string, string>? properties = null)
    {
        var processor = new EventProcessorClient(
            _blobContainerClient,
            _properties.GetOrDefault(EventHubs.GroupId) ?? EventHubConsumerClient.DefaultConsumerGroupName,
            _connectionString,
            topic.ToLower()
        );
        return new EventHubConsumer(processor, _properties.Merge(properties));
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