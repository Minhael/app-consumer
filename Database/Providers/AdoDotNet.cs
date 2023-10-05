using System.Data;
using System.Data.SqlClient;
using Common.Lang.Extensions;
using Common.Lang.Threading;
using Common.Telemetry;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Database.Providers;

public sealed class AdoDotNet : IDatabase
{
    private readonly string _name;
    private readonly string[] _hosts;
    private static readonly Tracer _tracer = Measure.CreateTracer<AdoDotNet>();

    public AdoDotNet(string name, params string[] connectionString)
    {
        _name = name;
        _hosts = connectionString;
    }

    public async Task<IDatabaseSession> OpenSession(CancellationToken cancellationToken = default)
    {
        return new AdoDotNetSession(await SelectConnection(cancellationToken));
    }

    internal Task<SqlConnectionCompat> SelectConnection(CancellationToken cancellationToken = default)
    {
        return Selector.Select(t => _hosts.Select(x => OpenConnection(_name, x, t)).ToArray(), cancellationToken);
    }

    private static async Task<SqlConnectionCompat> OpenConnection(string server, string connectionString, CancellationToken cancellationToken = default)
    {
        var conn = new SqlConnectionCompat(new SqlConnection(connectionString));
        using var span = _tracer.StartActiveSpan($"CONNECT {server}", SpanKind.Client);

        try
        {
            await conn.Connection.OpenAsync(cancellationToken);
            return conn;
        }
        catch (Exception e)
        {
            await conn.DisposeAsync();
            span.SetStatus(Status.Error);
            span.RecordException(e);
            throw;
        }
    }

    public override string? ToString()
    {
        return $"AdoDotNet.{_name}";
    }
}

internal sealed class AdoDotNetSession : IDatabaseSession
{
    private readonly SqlConnectionCompat _conn;
    private bool _isDisposed = false;

    public IDbConnection Connection => !_isDisposed ? _conn.Connection : throw new Exception("Connection is already disposed.");

    internal AdoDotNetSession(SqlConnectionCompat conn)
    {
        _conn = conn;
    }

    public void Dispose()
    {
        _isDisposed = true;
        _conn.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        await _conn.DisposeAsync();
    }
}

sealed class SqlConnectionCompat : IAsyncDisposable, IDisposable
{
    public readonly SqlConnection Connection;

    public SqlConnectionCompat(SqlConnection conn)
    {
        Connection = conn;
    }

    public void Dispose()
    {
        Connection.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
#if NETCOREAPP3_1_OR_GREATER
        await Connection.DisposeAsync();
#else
        Connection.Dispose();
        await Task.CompletedTask;
#endif
    }
}