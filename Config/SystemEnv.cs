namespace Common.Config;

/// <summary>
/// A IPropStore<string> implemented by system environment.
/// </summary>
public class SystemEnv : IPropStore<string>
{
    public static string? GetEnv(string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    public string? Get(string key)
    {
        return GetEnv(key);
    }

    public void Put(string key, string? value)
    {
        Environment.SetEnvironmentVariable(key, value);
    }

    public bool Has(string key)
    {
        return Environment.GetEnvironmentVariable(key) != null;
    }

    public void Clear()
    {
        throw new InvalidOperationException("Cannot clear env props");
    }
};