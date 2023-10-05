using System.Collections;
using System.Runtime.CompilerServices;

namespace Common.Lang.Transform;

public class TransformMonad<TSource, TResult> : IEnumerable<TResult>
    where TResult : class
{
    private readonly TSource[] _sources;
    private readonly TResult[] _results;

    public TransformMonad(TSource[]? sources, TResult[]? results)
    {

        _sources = sources ?? Array.Empty<TSource>();
        _results = results ?? Array.Empty<TResult>();
    }

    public IEnumerator<TResult> GetEnumerator()
    {
        return _results.AsEnumerable()
                        .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<TResult> Zip(Func<TSource, TResult, TResult> func)
    {
        return _sources.Zip(_results, func);
    }


    public IEnumerable<TResult> Zip(Func<TSource, TResult, int, TResult> func)
    {
        var indx = 0;
        return _sources.Zip(_results, (s, t) => func(s, t, indx++));
    }

    public IEnumerable<TResult> Zip<T>(T[] arr, Func<TSource, TResult, T, TResult> func)
    {
        var indx = 0;
        return _sources.Zip(_results, (s, t) => func(s, t, arr[indx++]));
    }

    public async IAsyncEnumerable<TResult> ZipAsync(Func<TSource, TResult, Task<TResult>> func, [EnumeratorCancellation] CancellationToken token = default)
    {
        foreach (var pair in _sources.Zip(_results, (a, b) => new { Src = a, Tgt = b }))
        {
            yield return await func(pair.Src, pair.Tgt);
        }
    }


    public IEnumerable<TResult> ZipMap(Action<TSource, TResult> action)
    {
        return _sources.Zip(_results, (source, result) =>
        {
            action(source, result);
            return result;
        });
    }

    public IEnumerable<TResult> ZipMap(Action<TSource, TResult, int> action)
    {
        var indx = 0;
        return _sources.Zip(_results, (source, result) =>
        {
            action(source, result, indx++);
            return result;
        });
    }

    public IEnumerable<TResult> ZipMap<T>(T[] arr, Action<TSource, TResult, T> action)
    {
        var indx = 0;
        return _sources.Zip(_results, (source, result) =>
        {
            action(source, result, arr[indx++]);
            return result;
        });
    }
}