using Common.Lang;
using Common.Lang.Extensions;
using Common.Telemetry;
using OpenTelemetry.Trace;

namespace Common.Dispatcher;

/// <summary>
/// Dispatch objects locally to registered IHandlers.
/// </summary>
public class LocalDispatcher : IDispatcher
{
    private static readonly Tracer _tracer = Measure.CreateTracer<LocalDispatcher>();

    private readonly IDictionary<Type, List<IHandler>> _dict;
    private readonly object _dictLock = new();

    public LocalDispatcher(IEnumerable<IHandler> handlers)
    {
        _dict = handlers.GroupBy(it => it.Type)
                        .ToDictionary(it => it.Key, it => it.ToList());
    }

    public bool HasHandler(Type type)
    {
        return GetHandlers(type).Any();
    }

    public virtual async Task Dispatch(object obj, CancellationToken token = default)
    {
        using var span = _tracer.StartActiveSpan($"{obj.GetType().FullName} dispatch", SpanKind.Internal);
        await Task.WhenAll(GetHandlers(obj.GetType()).MapNotNull(it => it.Handle(obj, token)));
    }

    public Task Dispatch(IEnumerable<object> obj, CancellationToken token = default)
    {
        return Task.WhenAll(obj.Select(x => Dispatch(x, token)));
    }

    public virtual IDisposable Register(IHandler handler)
    {
        AddHandler(handler);
        return new DisposableAction(() => RemoveHandler(handler));
    }

    private void AddHandler(IHandler handler)
    {
        lock (_dictLock)
        {
            var list = _dict.GetOrPut(handler.Type, () => new List<IHandler>());
            if (!list.Contains(handler))
                list.Add(handler);
        }
    }

    private void RemoveHandler(IHandler handler)
    {
        lock (_dictLock)
        {
            _dict.GetOrDefault(handler.Type)?.Remove(handler);
        }
    }

    private IEnumerable<IHandler> GetHandlers(Type type)
    {
        var types = type.GetAssignableTypes();
        lock (_dictLock)
        {
            return types.Select(x => _dict.GetOrDefault(x) ?? Enumerable.Empty<IHandler>()).Flatten().ToArray();
        }
    }
}