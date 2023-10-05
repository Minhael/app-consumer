namespace Common.Config;

/// <summary>
/// Interface to read/write from a DB cache, e.g.: Redis.
/// </summary>
public interface ICache
{

    #region Generic Methods

    Task<T?> Read<T>(string key, CancellationToken token = default);

    // The default value allows the caller to get some default value back if the key is not found.
    Task<T> Read<T>(string key, T defValue, CancellationToken token = default);
    Task<T> Read<T>(string key, Func<Task<T>> task, CancellationToken token = default);
    Task<T> Read<T>(string key, Func<Task<T>> task, TimeSpan effective, CancellationToken token = default);

    // When overwriting a value, we may want to return the previous value. If there is no previous value, return null.
    // To write a null value would mean to delete it. 
    Task Write<T>(string key, T? value, CancellationToken token = default);
    Task Write<T>(string key, T? value, TimeSpan effective, CancellationToken token = default);

    [Obsolete("Use Write(\"key\", null)")]
    Task Delete(string key, CancellationToken token = default) => Write<object>(key, null, token);

    #endregion Generic Methods

    #region Other Methods

    Task<bool> Has(string key, CancellationToken token = default);

    #endregion Other Methods
}