using System.Threading.Channels;

namespace Common.Messenger.Local;

//  https://github.com/open-telemetry/opentelemetry-dotnet/blob/f471a9f197d797015123fe95d3e12b6abf8e1f5f/examples/MicroserviceExample/Utils/Messaging/MessageReceiver.cs#L53
public sealed class LocalMessenger : IMessenger
{
    public static LocalMessenger Build(params string[] topics)
    {
        return new LocalMessenger(topics, topics.ToDictionary(x => x, x => Channel.CreateUnbounded<Message>()));
    }

    public string[] Topics { get; init; } = Array.Empty<string>();
    private readonly IReadOnlyDictionary<string, Channel<Message>> _channels;

    public LocalMessenger(string[] topics, IReadOnlyDictionary<string, Channel<Message>> channels)
    {
        Topics = topics;
        _channels = channels;
    }

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