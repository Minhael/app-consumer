using System.Data;
using Common.Serialization.Json;

namespace Common.Lang.Transform;

public static class TransformExtensions
{

    //  The ability is taken away by MS intentionally: 
    //  https://github.com/dotnet/runtime/issues/41920
    //  https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/dataset-datatable-dataview/security-guidance
    [Obsolete("Security breach. New implementation should not (de)serialize DataTable or DataSet directly: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/dataset-datatable-dataview/security-guidance")]
    public static TransformMonad<DataRow, TResult> Transform<TResult>(this DataTable dt, IJsonizer? jsonizer = null)
            where TResult : class
    {
        if (jsonizer == null) jsonizer = Newtonizer.Instance;
        var json = jsonizer.Serialize(dt);
        var result = jsonizer.Deserialize<TResult[]>(json);

        return new TransformMonad<DataRow, TResult>(sources: dt.AsEnumerable().ToArray(),
                                                    results: result);
    }

    public static TransformMonad<dynamic, TResult> Transform<TResult>(this string json, IJsonizer? jsonizer = null)
        where TResult : class
    {
        if (jsonizer == null) jsonizer = JsonExtensions.Default;
        var result = jsonizer.Deserialize<TResult[]>(json);
        return Transform<dynamic, TResult>(result, jsonizer);
    }

    public static TransformMonad<dynamic, TResult> Transform<TResult>(this object obj)
        where TResult : class
    {

        return Transform<dynamic, TResult>(new[] { obj });
    }

    public static TransformMonad<dynamic, TResult> Transform<TResult>(this dynamic[] obj)
        where TResult : class
    {
        return Transform<dynamic, TResult>(obj);
    }

    public static TransformMonad<dynamic, TResult> Transform<TResult>(this IEnumerable<dynamic> obj)
        where TResult : class
    {
        return Transform<dynamic, TResult>(obj.ToArray());
    }

    public static TransformMonad<TSource, TResult> Transform<TSource, TResult>(this TSource source)
        where TResult : class
    {
        var arr = new[] { source };
        return Transform<TSource, TResult>(arr);
    }

    public static TransformMonad<TSource, TResult> Transform<TSource, TResult>(this TSource[] source, IJsonizer? jsonizer = null)
        where TResult : class
    {
        var result = source.ComposeJson(jsonizer).ParseJsonAs<TResult[]>(jsonizer);
        return new TransformMonad<TSource, TResult>(source, result);
    }

    public static TransformMonad<TSource, TResult> Transform<TSource, TResult>(this IEnumerable<TSource> source)
        where TResult : class
    {
        return Transform<TSource, TResult>(source.ToArray());
    }
}