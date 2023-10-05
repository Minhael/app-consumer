namespace Common.Lang.Threading;

/// <summary>
/// Wrap task as disposable with auto cancellation.
/// </summary>
public sealed class DedicatedTask : IAsyncDisposable
{
    public static DedicatedTask Run(Func<CancellationToken, Task> builder)
    {
        var source = new CancellationTokenSource();
        return new DedicatedTask(Task.Run(() => builder(source.Token)), source);
    }

    private readonly Task _task;
    private readonly CancellationTokenSource _source;
    public CancellationToken CancellationToken { get => _source.Token; }

    public DedicatedTask(Task task, CancellationTokenSource source)
    {
        _task = task;
        _source = source;
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);
    }

    private async ValueTask DisposeAsyncCore()
    {
        _source.Cancel();
        await _task;
        _source.Dispose();
    }
}

public static class DisposableTaskExtensions
{
    public static async Task WaitUntilCancelled(this IAsyncDisposable[] self, CancellationToken token = default)
    {
        try
        {
            await Task.Delay(Timeout.Infinite, token);
        }
        catch (TaskCanceledException) { }
        catch (OperationCanceledException) { }
        finally
        {
            await Task.WhenAll(self.Select(x => x.DisposeAsync().AsTask())).ConfigureAwait(false);
        }
    }
}