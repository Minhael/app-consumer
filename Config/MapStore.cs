using Common.Lang.Extensions;
using Common.Serialization.Json;

namespace Common.Config;

/// <summary>
/// A IPropStore<string> implemented by heap memory.
/// </summary>
public class MapStore : IPropStore<string>
{
    public static MapStore FromJson(string json)
    {
        return new MapStore(json.ParseJsonAs<Dictionary<string, object>>().Map(Convert));
    }

    private static string Convert(object obj)
    {
        return obj switch
        {
            string str => str,
            Type { IsPrimitive: false } => obj.ComposeJson(),
            //  Don't know what it is, delay exception to call site
            _ => obj.ToString() ?? ""
        };
    }

    private readonly IDictionary<string, string> _map;

    public MapStore(IDictionary<string, string> map)
    {
        _map = map;
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public string? Get(string key)
    {
        return _map.GetOrDefault(key);
    }

    public bool Has(string key)
    {
        return _map.ContainsKey(key);
    }

    public void Put(string key, string? value)
    {
        if (value == null)
            _map.Remove(key);
        else
            _map[key] = value;
    }
}