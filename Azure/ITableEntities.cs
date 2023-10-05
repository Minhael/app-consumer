using System.Runtime.CompilerServices;
using Azure;
using Azure.Data.Tables;
using Common.Lang.Extensions;
using Common.Serialization.Json;
using Common.Telemetry;
using OpenTelemetry.Trace;
using Serilog;

namespace Common.Azure;
public interface ITableEntities
{
    IQueryBuilder<T> Query<T>() where T : class;
    IOperationBuilder Edit();

    Task<(T, string)> GetByKey<T>(string partitionKey, string rowKey, CancellationToken token = default) where T : class;
    [Obsolete("Please consider Update with ETag or Upsert without ETag.")]
    Task<string> Upsert(string partitionKey, string rowKey, string etag, object obj, CancellationToken token = default);
    Task<string> Update(string partitionKey, string rowKey, string etag, object obj, CancellationToken token = default);
    Task<string> Upsert(string partitionKey, string rowKey, object obj, CancellationToken token = default);
    Task Delete(string partitionKey, string rowKey, CancellationToken token = default);
}

public interface IQueryBuilder<T> where T : class
{
    void ODataId(string partitionKey, string rowKey);
    void PartitionKey(params string[] partitionKey);
    void RowKey(params string[] rowKey);
    void Filters(params string[] filters);
    void Project(params string[] fields);
    void StartsWith(string column, string prefix, string op = "and");
    IAsyncEnumerable<(T, string)> Execute(CancellationToken token = default);
}

public interface IOperationBuilder : IDisposable
{
    void Upsert(string partitionKey, string rowKey, object obj);

    void Delete(string partitionKey, string rowKey);

    Task Execute(CancellationToken token = default);
}

//  https://medium.com/geekculture/using-the-new-c-azure-data-tables-sdk-with-azure-cosmos-db-786085ac8190
class TableEntities : ITableEntities
{
    private static readonly Tracer _tracer = Measure.CreateTracer<TableEntities>();

    private readonly TableClient _table;

    public TableEntities(TableClient table)
    {
        _table = table;
    }

    public IOperationBuilder Edit()
    {
        return new Batch(_table);
    }

    public IQueryBuilder<T> Query<T>() where T : class
    {
        return new TableQuery<T>(_table);
    }

    public async Task<(T, string)> GetByKey<T>(string partitionKey, string rowKey, CancellationToken token = default) where T : class
    {
        var query = Query<T>();
        query.ODataId(partitionKey, rowKey);

        var attr = new SpanAttributes();
        attr.Add("table.partition", partitionKey);
        attr.Add("table.row", rowKey);
        using var span = _tracer.StartActiveSpan($"GET table.{_table.Name}", SpanKind.Client, initialAttributes: attr);
        try
        {
            return await query.Execute(token).FirstAsync(token);
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentOutOfRangeException($"Entity not found: PartitionKey = {partitionKey} RowKey = {rowKey}", e);
        }
    }

    public async Task<string> Update(string partitionKey, string rowKey, string etag, object obj, CancellationToken token = default)
    {
        if (etag.IsNullOrWhiteSpace())
            throw new ArgumentException("ETag cannot be null or empty");
        var attr = new SpanAttributes();
        attr.Add("table.etag", etag);
        attr.Add("table.partition", partitionKey);
        attr.Add("table.row", rowKey);
        using var span = _tracer.StartActiveSpan($"UPDATE table.{_table.Name}", SpanKind.Client, initialAttributes: attr);
        return await UpdateAsync(partitionKey, rowKey, etag, obj, token);
    }

    [Obsolete("Please consider Update with ETag or Upsert without ETag.")]
    public async Task<string> Upsert(string partitionKey, string rowKey, string etag, object obj, CancellationToken token = default)
    {
        if (etag.IsNullOrWhiteSpace())
            throw new ArgumentException("ETag cannot be null or empty");
        var attr = new SpanAttributes();
        attr.Add("table.etag", etag);
        attr.Add("table.partition", partitionKey);
        attr.Add("table.row", rowKey);
        using var span = _tracer.StartActiveSpan($"UPDATE table.{_table.Name}", SpanKind.Client, initialAttributes: attr);
        return await UpsertAsync(partitionKey, rowKey, obj, token);
    }

    public async Task<string> Upsert(string partitionKey, string rowKey, object obj, CancellationToken token = default)
    {
        var attr = new SpanAttributes();
        attr.Add("table.partition", partitionKey);
        attr.Add("table.row", rowKey);
        using var span = _tracer.StartActiveSpan($"UPSERT table.{_table.Name}", SpanKind.Client, initialAttributes: attr);
        return await UpsertAsync(partitionKey, rowKey, obj, token);
    }

    public async Task Delete(string partitionKey, string rowKey, CancellationToken token = default)
    {
        var attr = new SpanAttributes();
        attr.Add("table.partition", partitionKey);
        attr.Add("table.row", rowKey);
        using var span = _tracer.StartActiveSpan($"DELETE table.{_table.Name}", SpanKind.Client, initialAttributes: attr);
        await _table.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: token);
    }

    private async Task<string> UpdateAsync(string partitionKey, string rowKey, string etag, object obj, CancellationToken token = default)
    {
        var entity = new TableEntity(obj.ComposeJson().ParseJsonAs<Dictionary<string, object>>())
        {
            PartitionKey = partitionKey,
            RowKey = rowKey
        };
        await _table.UpdateEntityAsync(entity, new ETag(etag), TableUpdateMode.Replace, token);
        var result = await _table.GetEntityAsync<TableEntity>(partitionKey, rowKey, cancellationToken: token);
        return result.Value.ETag.ToString();
    }

    private async Task<string> UpsertAsync(string partitionKey, string rowKey, object obj, CancellationToken token = default)
    {
        var entity = new TableEntity(obj.ComposeJson().ParseJsonAs<Dictionary<string, object>>())
        {
            PartitionKey = partitionKey,
            RowKey = rowKey
        };
        await _table.UpsertEntityAsync(entity, TableUpdateMode.Replace, token);
        var result = await _table.GetEntityAsync<TableEntity>(partitionKey, rowKey, cancellationToken: token);
        return result.Value.ETag.ToString();
    }

    class TableQuery<T> : IQueryBuilder<T> where T : class
    {
        private readonly TableClient _table;
        public string? Filter;
        public string[]? Projection;
        private static readonly Serilog.ILogger _logger = Log.ForContext<TableEntities>();

        public TableQuery(TableClient table)
        {
            _table = table;
        }

        public async IAsyncEnumerable<(T, string)> Execute([EnumeratorCancellation] CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(Filter))
            {
                var attr = new SpanAttributes();
                attr.Add("table.filter", Filter);
                attr.Add("table.projection", Projection);
                using var span = _tracer.StartActiveSpan($"QUERY table.{_table.Name}", SpanKind.Client, initialAttributes: attr);

                var query = _table
                    .QueryAsync<TableEntity>(Filter, select: Projection, cancellationToken: token)
                    .OrderByDescending(it => it.Timestamp)
                    .Select(it => (it.ComposeJson().ParseJsonAs<T>(), it.ETag.ToString()));

                await foreach (var i in query)
                    yield return i;
            }
            else
            {
                _logger.Warning("Empty query filter. Possibly due to empty list of partition keys or row keys.");
            }
        }

        public void StartsWith(string column, string prefix, string op = "and")
        {
            Filter = ConcatQuery($"{column} ge '{prefix}' and {column} lt '{NextKey(prefix)}'", op);
        }

        public void Filters(params string[] filters)
        {
            if (filters.Length > 0)
            {
                Filter = ConcatQuery($"({filters.JoinString(") and (")})");
            }
        }

        public void ODataId(string partitionKey, string rowKey)
        {
            Filter = ConcatQuery(TableClient.CreateQueryFilter<TableEntity>(x => x.PartitionKey == partitionKey && x.RowKey == rowKey), "or");
        }

        public void PartitionKey(params string[] partitionKey)
        {
            if (partitionKey.Length > 0)
            {
                var filters = partitionKey.Select(x => TableClient.CreateQueryFilter<TableEntity>(y => y.PartitionKey == x)).ToArray();
                Filter = ConcatQuery(filters.JoinString(" or "));
            }
        }

        public void RowKey(params string[] rowKey)
        {
            if (rowKey.Length > 0)
            {
                var filters = rowKey.Select(x => TableClient.CreateQueryFilter<TableEntity>(y => y.RowKey == x)).ToArray();
                Filter = ConcatQuery(filters.JoinString(" or "));
            }
        }

        internal string ConcatQuery(string filter, string op = "and")
        {
            return Filter == null ? filter : $"({Filter}) {op} ({filter})";
        }

        public void Project(params string[] fields)
        {
            if (fields.Any())
                Projection = fields;
            else
                Projection = null;
        }

        private static string NextKey(string startsWith)
        {
            var length = startsWith.Length - 1;
            var nextChar = startsWith[length] + 1;
            return startsWith.Substring(0, length) + (char)nextChar;
        }
    }

    sealed class Batch : IOperationBuilder
    {
        private readonly TableClient _table;
        private readonly Dictionary<string, List<TableTransactionAction>> _execution = new();

        private static readonly Serilog.ILogger _logger = Log.ForContext<TableEntities>();

        public Batch(TableClient table)
        {
            _table = table;
        }

        public void Upsert(string partitionKey, string rowKey, object obj)
        {
            var entity = new TableEntity(obj.ComposeJson().ParseJsonAs<Dictionary<string, object>>())
            {
                PartitionKey = partitionKey,
                RowKey = rowKey
            };

            _execution
                .GetOrPut(partitionKey, () => new List<TableTransactionAction>())
                .Add(new TableTransactionAction(TableTransactionActionType.UpdateReplace, entity));
        }

        public void Delete(string partitionKey, string rowKey)
        {
            var entity = new TableEntity(partitionKey, rowKey);
            _execution
                .GetOrPut(partitionKey, () => new List<TableTransactionAction>())
                .Add(new TableTransactionAction(TableTransactionActionType.Delete, entity));
        }

        public async Task Execute(CancellationToken token = default)
        {
            //  https://www.andybutland.dev/2018/11/azure-table-storage-batch-insert-within-operations-limit.html

            if (!_execution.Any())
                return;

            var tasks = _execution
                .Select(it => it.Value)
                .Select(it => it.Chunk(100))
                .Flatten()
                .Select(it => _table.SubmitTransactionAsync(it, token));

            using var span = _tracer.StartActiveSpan($"EXECUTE table.{_table.Name}", SpanKind.Client);

            await Task.WhenAll(tasks);
        }

        public void Dispose()
        {
            _execution.Clear();
        }
    }
}

static class EntitiesExtensions
{
    public static string CombineFilters(this IEnumerable<string> self, string op)
    {
        return string.Join($" {op} ", self.Select(it => $"({it})"));
    }
}