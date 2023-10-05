namespace Common.Dispatcher;

/// <summary>
/// Dispatch objects to an other dispatcher.
/// </summary>
public class ProxyDispatcher : IDispatcher
{
    public static ProxyDispatcher FromAction(Func<IDispatcher> action) => new ProxyDispatcher(new Lazy<IDispatcher>(action));

    //  Handler may use IDispatcher and create a circular dependencies
    //  Safe to use Lazy to break the circle
    private readonly Lazy<IDispatcher> _dispatcher;

    public ProxyDispatcher(Lazy<IDispatcher> dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public bool HasHandler(Type type) => _dispatcher.Value.HasHandler(type);
    public Task Dispatch(object obj, CancellationToken token = default) => _dispatcher.Value.Dispatch(obj, token);
    public Task Dispatch(IEnumerable<object> obj, CancellationToken token = default) => _dispatcher.Value.Dispatch(obj, token);
    public IDisposable Register(IHandler handler) => _dispatcher.Value.Register(handler);

    public override string? ToString()
    {
        return $"{base.ToString()} {{ {_dispatcher.Value} }}";
    }
}