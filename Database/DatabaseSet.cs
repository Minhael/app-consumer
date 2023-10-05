using Common.Lang.Extensions;

namespace Common.Database;

public class DatabaseSet : IDatabase
{
    private readonly string _default;
    private readonly IDictionary<string, IDatabase> _availables;

    public DatabaseSet(string @default, IDictionary<string, IDatabase> availables)
    {
        _default = @default;
        _availables = availables;
    }

    public string[] Keys() => _availables.Keys.ToArray();

    public IDatabase ForName(string? key)
    {
        if (key != null)
            return _availables.GetOrDefault(key) ?? throw new ArgumentException($"No such database with connection key={key}");
        if (_availables.ContainsKey(_default))
            return _availables.GetOrDefault(_default) ?? throw new ArgumentException($"No such database with connection key={_default}");
        throw new InvalidOperationException($"No database is selected.");
    }

    public Task<IDatabaseSession> OpenSession(string key, CancellationToken cancellationToken = default) => ForName(key).OpenSession(cancellationToken);
    public Task<IDatabaseSession> OpenSession(CancellationToken cancellationToken = default)
    {
        if (_availables.ContainsKey(_default))
            return OpenSession(_default, cancellationToken);
        throw new InvalidOperationException($"No database is selected.");
    }
}

public static class DatabaseSetExtensions
{
    public static IDatabase ForConnection(this IDatabase self, string name)
    {
        if (self is DatabaseSet s)
            return s.ForName(name);
        throw new ArgumentException($"Unable to select database {name}. The database may already be selected or this application does not have multiple database connections.");
    }
}