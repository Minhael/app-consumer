namespace Common.Config;

/// <summary>
/// Interface to persist/read serialized property value in format T.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPropStore<T>
{
    T? Get(string key);
    void Put(string key, T? value);
    bool Has(string key);
    void Clear();
}