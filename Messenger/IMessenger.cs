namespace Common.Messenger;

public interface IMessenger : IDisposable, IAsyncDisposable
{
    string[] Topics { get; }
    IProducer Produce(string topic, IDictionary<string, string>? properties = null);
    IConsumer Consume(string topic, IDictionary<string, string>? properties = null);
}