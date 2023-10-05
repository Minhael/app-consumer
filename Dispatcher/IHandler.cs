using Common.Lang.Extensions;

namespace Common.Dispatcher;

/// <summary>
/// Interface to perform action on a dispatched objects.
/// </summary>
public interface IHandler
{
    Type Type { get; }
    Task? Handle(object obj, CancellationToken token = default);
}

public abstract class Handler<T> : IHandler where T : class
{
    public Type Type => typeof(T);

    protected abstract Task? Handle(T obj, CancellationToken token = default);

    public Task? Handle(object obj, CancellationToken token = default)
    {
        return obj.UnsafeCast<T>().Let(it => Handle(it, token));
    }
}