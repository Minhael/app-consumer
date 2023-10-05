namespace Common.Dispatcher;

/// <summary>
/// Interface to execute registered logic on dispatching objects.
/// </summary>
public interface IDispatcher
{
    bool HasHandler(Type type);
    Task Dispatch(object obj, CancellationToken token = default);
    Task Dispatch(IEnumerable<object> obj, CancellationToken token = default);
    IDisposable Register(IHandler handler);
}