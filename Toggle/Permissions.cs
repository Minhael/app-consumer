using System.Collections.ObjectModel;
using System.ComponentModel;
using Common.Lang.Extensions;

namespace Common.Toggle;

public class Permissions : ReadOnlyDictionary<string, string?>
{

    public Permissions(IDictionary<string, string?> permissions) : base(permissions)
    {
    }

    public string? GetValue(string key, string? defValue = default)
    {
        if (!this.ContainsKey(key))
            return defValue;

        return ((IReadOnlyDictionary<string, string?>) this).GetOrDefault(key);
    }

    public T? GetValue<T>(string key, T? defValue = null) where T : struct
    {
        if (!this.ContainsKey(key))
            return defValue;

        var value = ((IReadOnlyDictionary<string, string?>) this).GetOrDefault(key);

        if (value == null)
            return null;

        try
        {
            return (T?)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(value);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
