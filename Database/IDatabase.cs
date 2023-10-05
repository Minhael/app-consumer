using System.Data;

namespace Common.Database;

public interface IDatabase
{
    Task<IDatabaseSession> OpenSession(CancellationToken cancellationToken = default);
}

public interface IDatabaseSession : IAsyncDisposable, IDisposable
{
    IDbConnection Connection { get; }
}

public static class DatabaseExtensions
{
    public static async Task<T> OpenConnection<T>(this IDatabase self, Func<IDbConnection, CancellationToken, Task<T>> execution, CancellationToken cancellationToken = default)
    {
        await using var session = await self.OpenSession(cancellationToken);
        return await execution(session.Connection, cancellationToken);
    }
}