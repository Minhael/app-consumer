namespace Common.Lang;

public static class Disposables
{
    public static IDisposable Deferred(Action action) => new DisposableAction(action);
    public static IAsyncDisposable DeferredAsync(Func<Task> fn) => new AsyncDisposableAction(fn);
}

sealed class DisposableAction : IDisposable
{
    private readonly Action action;

    public DisposableAction(Action action)
    {
        this.action = action;
    }

    public void Dispose()
    {
        action();
    }
}

sealed class AsyncDisposableAction : IAsyncDisposable
{
    private readonly Func<Task> _task;

    public AsyncDisposableAction(Func<Task> task)
    {
        _task = task;
    }

    public async ValueTask DisposeAsync()
    {
        await _task();
    }
}