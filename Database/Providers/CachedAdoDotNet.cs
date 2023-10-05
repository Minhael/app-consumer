using System.Data;
using System.Data.SqlClient;

namespace Common.Database.Providers;

//  https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/enabling-multiple-active-result-sets
public sealed class CachedAdoDotNet : IDatabase, IDisposable, IAsyncDisposable
{
    private readonly AdoDotNet _database;

    private SqlConnection? _conn = null;
    private bool _isDisposed = true;

    private readonly SemaphoreSlim semaphor = new(1, 1);

    public CachedAdoDotNet(AdoDotNet database)
    {
        _database = database;
    }

    public async Task<IDatabaseSession> OpenSession(CancellationToken cancellationToken = default)
    {
        var conn = await GetConnection(cancellationToken);
        return new CachedAdoDotNetSession(conn);
    }

    public void Dispose()
    {
        semaphor.Wait();
        try
        {
            RenewConnection(null);
        }
        finally
        {
            semaphor.Release();
        }
        semaphor.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await semaphor.WaitAsync();
        try
        {
            RenewConnection(null);
        }
        finally
        {
            semaphor.Release();
        }
        semaphor.Dispose();
    }

    private async Task<SqlConnection> GetConnection(CancellationToken cancellationToken = default)
    {
        SqlConnection conn;

        await semaphor.WaitAsync(cancellationToken);
        try
        {
            if (!_isDisposed && _conn != null)
            {
                conn = _conn;
            }
            else
            {
                var wrapper = await _database.SelectConnection(cancellationToken);
                conn = wrapper.Connection;
                RenewConnection(conn);
            }
        }
        finally
        {
            semaphor.Release();
        }

        return conn;
    }

    private void RenewConnection(SqlConnection? conn)
    {
        if (_conn == conn) return;
        _conn?.Dispose();
        _conn = conn;
        _isDisposed = _conn == null;
    }
}

internal sealed class CachedAdoDotNetSession : IDatabaseSession
{
    private readonly IDbConnection _conn;
    public IDbConnection Connection => _conn;

    public CachedAdoDotNetSession(IDbConnection conn)
    {
        _conn = conn;
    }

    public void Dispose()
    {
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }
}