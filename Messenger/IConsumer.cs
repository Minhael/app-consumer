namespace Common.Messenger;

public interface IConsumer
{
    IAsyncDisposable Subscribe(Func<Message, CancellationToken, Task> subscription);
}