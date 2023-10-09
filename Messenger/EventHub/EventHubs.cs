using System.Collections.Immutable;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Common.Lang.Extensions;
using Common.Lang.IO;

namespace Common.Messenger.EventHub;

public static class EventHubs
{
    public const string GroupId = "groupID";
    public const string AutoOffsetReset = "auto.offset.reset";

    public record Configuration
    {
        public string BlobConnectionString { get; init; } = "";
        public string BlobContainerName { get; init; } = "";
        public string ConnectionString { get; init; } = "";
        public string[] Topics { get; init; } = Array.Empty<string>();
        public IDictionary<string, string> Properties { get; init; } = ImmutableDictionary<string, string>.Empty;
    }

    public static HubMessenger FromFile(string uri) => Build(FileX.ReadAs<Configuration>(uri));
    public static HubMessenger Build(Configuration cfg)
    {
        var producers = cfg.Topics.Select(x => new EventHubProducerClient(cfg.ConnectionString, x.ToLower())).ToDictionary(x => x.EventHubName.ToLower());
        var blobClient = new BlobContainerClient(cfg.BlobConnectionString, cfg.BlobContainerName);
        return new HubMessenger(producers, cfg.Topics, blobClient, cfg.ConnectionString, cfg.Properties);
    }
}

static class HubMessagerHelpers
{
    public static Message Convert(this ProcessEventArgs args)
    {
        var payload = args.Data.EventBody.ToArray().Utf8();
        var key = args.Data.PartitionKey;
        var utc = args.Data.EnqueuedTime.ToUnixTimeMilliseconds();
        var properties = new Dictionary<string, object>(args.Data.Properties);

        return new Message
        {
            Payload = payload,
            Key = key,
            Utc = utc,
            Properties = properties,
        };
    }
}