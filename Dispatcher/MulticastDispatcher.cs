namespace Common.Dispatcher;

public class MulticastDispatcher : IDispatcher
{
    private readonly IDispatcher[] _dispatchers;

    public MulticastDispatcher(IDispatcher[] dispatchers)
    {
        _dispatchers = dispatchers;
    }

    public Task Dispatch(object obj, CancellationToken token = default) => Task.WhenAll(_dispatchers.Select(x => x.Dispatch(obj, token)));

    public Task Dispatch(IEnumerable<object> obj, CancellationToken token = default)
    {
        return Task.WhenAll(obj.SelectMany(x => _dispatchers.Select(y => y.Dispatch(x, token))));
    }

    public bool HasHandler(Type type) => _dispatchers.Any(x => x.HasHandler(type));

    public IDisposable Register(IHandler handler)
    {
        throw new InvalidOperationException("Register handler is not supported by multicast dispatcher.");
    }
}