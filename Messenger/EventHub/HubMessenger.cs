using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Common.Lang.Extensions;
using Nito.AsyncEx.Synchronous;

namespace Common.Messenger.EventHub;

public sealed class HubMessenger : IMessenger
{
    private string BlobConnectionString { get; init; }
    private string BlobContainerName { get; init; }

    private string ConnectionString { get; init; }
    public string[] Topics { get; init; }
    private IDictionary<string, string> Properties { get; init; }

    private readonly IReadOnlyDictionary<string, EventHubProducerClient> _producers;

    public HubMessenger(string blobConnectionString, string blobContainerName, string connectionString, string[] topics, IEnumerable<KeyValuePair<string, string>> properties)
    {
        BlobConnectionString = blobConnectionString;
        BlobContainerName = blobContainerName;
        ConnectionString = connectionString;
        Topics = topics;
        Properties = new Dictionary<string, string>(properties);
        _producers = Topics.Select(x => new EventHubProducerClient(ConnectionString, x.ToLower())).ToDictionary(x => x.EventHubName);
    }

    public IConsumer Consume(string topic, IDictionary<string, string>? properties = null)
    {
        var blobClient = new BlobContainerClient(BlobConnectionString, BlobContainerName);
        var processor = new EventProcessorClient(
            blobClient,
            properties?.GetOrDefault(EventHubConsumerProps.GroupId) ?? EventHubConsumerClient.DefaultConsumerGroupName,
            ConnectionString,
            topic
        );
        return new EventHubConsumer(processor, this.Properties.Merge(properties));
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