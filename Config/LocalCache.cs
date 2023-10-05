namespace Common.Config;

/*
 *  This is a special class to cache remote cache value, providing a secondary cache locally.
 *  It may reduce some blusts in network usage.
 *
 *  If you want a cache that work locally in memory, please refer to MemCache.
 */
public class LocalCache : ICache
{
    private readonly ICache _cache;
    private readonly MemCache _mem;
    private readonly TimeSpan _expire;

    public LocalCache(ICache cache, MemCache mem, TimeSpan expire)
    {
        _cache = cache;
        _mem = mem;
        _expire = expire;
    }

    public Task<bool> Has(string key, CancellationToken token = default) => _cache.Has(key, token);

    public async Task<T?> Read<T>(string key, CancellationToken token = default)
    {
        if (_cache != _mem)
            return await _mem.Read<T?>(key, () => _cache.Read<T>(key, token), _expire, token);
        else
            return await _mem.Read<T>(key, token);
    }

    public Task<T> Read<T>(string key, T defValue, CancellationToken token = default)
    {
        if (_cache != _mem)
            return _mem.Read<T>(key, () => _cache.Read<T>(key, defValue, token), _expire, token);
        else
            return _mem.Read<T>(key, defValue, token);
    }

    public Task<T> Read<T>(string key, Func<Task<T>> task, CancellationToken token = default)
    {
        if (_cache != _mem)
            return _mem.Read<T>(key, () => _cache.Read<T>(key, task, token), _expire, token);
        else
            return _mem.Read<T>(key, task, token);
    }

    public Task<T> Read<T>(string key, Func<Task<T>> task, TimeSpan effective, CancellationToken token = default)
    {
        if (_cache != _mem)
            return _mem.Read<T>(key, () => _cache.Read<T>(key, task, effective, token), _expire, token);
        else
            return _mem.Read<T>(key, task, effective, token);
    }

    public Task Write<T>(string key, T? value, CancellationToken token = default)
    {
        if (_cache != _mem)
            return Task.WhenAll(
                _cache.Write<T>(key, value, token),
                _mem.Write<T>(key, value, token)
            );
        else
            return _mem.Write<T>(key, value, token);
    }

    public Task Write<T>(string key, T? value, TimeSpan effective, CancellationToken token = default)
    {
        if (_cache != _mem)
            return Task.WhenAll(
                _cache.Write<T>(key, value, effective, token),
                _mem.Write<T>(key, value, _expire, token)
            );
        else
            return _mem.Write<T>(key, value, _expire, token);
    }
}