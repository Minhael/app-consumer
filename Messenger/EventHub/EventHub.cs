using Azure.Messaging.EventHubs.Processor;
using Common.Lang.Extensions;
using NodaTime;

namespace Common.Messenger.EventHub;

public static class EventHubConsumerProps
{
    public const string GroupId = "groupID";
    public const string AutoOffsetReset = "auto.offset.reset";
}

static class HubMessagerHelpers
{
    public static Message Convert(this ProcessEventArgs args)
    {
        var payload = args.Data.EventBody.ToArray().Utf8();
        var key = args.Data.PartitionKey;
        var timestamp = Instant.FromUnixTimeMilliseconds(args.Data.EnqueuedTime.ToUnixTimeMilliseconds());
        var properties = new Dictionary<string, object>(args.Data.Properties);

        return new Message
        {
            Payload = payload,
            Key = key,
            Timestamp = timestamp,
            Properties = properties,
        };
    }
}