using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Common.Config;

public class MemCache : ICache
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public MemCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<bool> Has(string key, CancellationToken token = default)
    {
        await Task.CompletedTask;
        return _cache.Get(key) != null;
    }

    public async Task<T?> Read<T>(string key, CancellationToken token = default)
    {
        await Task.CompletedTask;
        return _cache.Get<T>(key);
    }

    public async Task<T> Read<T>(string key, T defValue, CancellationToken token = default)
    {
        await Task.CompletedTask;
        return _cache.GetOrCreate<T>(key, x => defValue);
    }

    public async Task<T> Read<T>(string key, Func<Task<T>> task, CancellationToken token = default)
    {
        //  Hit
        if (_cache.TryGetValue(key, out T value))
            return value;

        //  Miss
        var l = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await l.WaitAsync(token);
        try
        {
            return await _cache.GetOrCreateAsync<T>(key, _ => task());
        }
        finally
        {
            l.Release();
            _locks.Remove(key, out _);
        }
    }

    public async Task<T> Read<T>(string key, Func<Task<T>> task, TimeSpan effective, CancellationToken token = default)
    {
        //  Hit
        if (_cache.TryGetValue(key, out T value))
            return value;

        //  Miss
        var l = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await l.WaitAsync(token);
        try
        {
            if (_cache.TryGetValue(key, out value))
                return value;

            value = await task();
            await Write(key, value, effective, token);
            return value;
        }
        finally
        {
            l.Release();
            _locks.Remove(key, out _);
        }
    }

    public async Task Write<T>(string key, T? value, CancellationToken token = default)
    {
        await Task.CompletedTask;
        if (value == null)
            _cache.Remove(key);
        else
            _cache.Set<T>(key, value);
    }

    public async Task Write<T>(string key, T? value, TimeSpan effective, CancellationToken token = default)
    {
        await Task.CompletedTask;
        if (value == null)
            _cache.Remove(key);
        else
            _cache.Set<T>(key, value, effective);
    }
}