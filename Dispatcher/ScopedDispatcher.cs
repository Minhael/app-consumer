using Common.Lang;
using Common.Lang.Extensions;

namespace Common.Dispatcher;

public class ScopedDispatcher : LocalDispatcher
{
    public sealed class Factory : IDisposable
    {
        private readonly Dictionary<string, ScopedDispatcher> _dispatchers = new();
        private readonly object _dictLock = new();

        public IDispatcher Get(string key, IEnumerable<IHandler>? handlers = null)
        {
            return new DeferredDispatcher(_dictLock, () => GetOrCreate(key, handlers));
        }

        public IEnumerable<string> Keys()
        {
            lock (_dictLock)
            {
                return _dispatchers.Keys;
            }
        }

        private ScopedDispatcher GetOrCreate(string key, IEnumerable<IHandler>? handlers = null)
        {
            lock (_dictLock)
            {
                return _dispatchers.GetOrDefault(key) ?? new ScopedDispatcher(
                    handlers ?? Enumerable.Empty<IHandler>(),
                    (o) => { lock (_dictLock) { _dispatchers.Add(key, o); } },
                    () => { lock (_dictLock) { _dispatchers.Remove(key); } }
                );
            }
        }

        public void Dispose()
        {
            lock (_dictLock)
            {
                _dispatchers.Clear();
            }
        }
    }

    private readonly Action<ScopedDispatcher> _onRegister;
    private readonly Action _onUnregister;

    private readonly object _refCountLock = new();
    private int _refCount = 0;

    internal ScopedDispatcher(IEnumerable<IHandler> handlers, Action<ScopedDispatcher> onRegister, Action onUnregister) : base(handlers)
    {
        _onRegister = onRegister;
        _onUnregister = onUnregister;
    }

    public override IDisposable Register(IHandler handler)
    {
        var disposable = base.Register(handler);
        Increment();
        return new DisposableAction(() =>
        {
            Decrement();
            disposable.Dispose();
        });
    }

    private void Increment()
    {
        lock (_refCountLock)
        {
            var isRegister = _refCount == 0;
            _refCount++;
            if (isRegister)
                _onRegister(this);
        }
    }

    private void Decrement()
    {
        lock (_refCountLock)
        {
            _refCount--;
            if (_refCount < 1)
                _onUnregister();
        }
    }

    private class DeferredDispatcher : IDispatcher
    {

        private readonly object _lock;

        private readonly Func<IDispatcher> _deferred;

        public DeferredDispatcher(object @lock, Func<IDispatcher> deferred)
        {
            _lock = @lock;
            _deferred = deferred;
        }

        public Task Dispatch(object obj, CancellationToken token = default) => _deferred().Dispatch(obj, token);
        public Task Dispatch(IEnumerable<object> obj, CancellationToken token = default) => _deferred().Dispatch(obj, token);
        public bool HasHandler(Type type) => _deferred().HasHandler(type);
        public IDisposable Register(IHandler handler)
        {
            lock (_lock)
            {
                return _deferred().Register(handler);
            }
        }
    }
}