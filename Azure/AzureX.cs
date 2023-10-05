namespace Common.Azure;

public static class AzureX
{
    public static IDictionary<string, string> Parse(string connStr)
    {
        return connStr
            .Split(';')
            .Select(it => it.Split('='))
            .Where(it => it.Length > 1)
            .ToDictionary(
                s => s[0],
                s => s[1],
                StringComparer.OrdinalIgnoreCase
            );
    }
}