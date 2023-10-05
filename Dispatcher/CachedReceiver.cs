using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Common.Dispatcher;

public class CachedReceiver<T> : Handler<T>, IDisposable where T : class
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();

    public async IAsyncEnumerable<T> Observe([EnumeratorCancellation] CancellationToken token = default)
    {
        while (!token.IsCancellationRequested && await _channel.Reader.WaitToReadAsync(token))
        {
            yield return await _channel.Reader.ReadAsync(token);
        }
    }

    public bool Stop()
    {
        return _channel.Writer.TryComplete();
    }

    protected override async Task Handle(T obj, CancellationToken token = default)
    {
        if (await _channel.Writer.WaitToWriteAsync(token))
            await _channel.Writer.WriteAsync(obj, token);
    }

    public void Dispose() => Stop();
}