namespace Common.Config;

/// <summary>
/// Interface to get/set type-safe property values with a string key.
/// </summary>
public interface IProps
{
    T Get<T>(string key);
    T Get<T>(string key, T defValue);
    T? Put<T>(string key, T? value);
    bool Has(string key);
    void Clear();
}