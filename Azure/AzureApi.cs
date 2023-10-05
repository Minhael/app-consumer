using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Common.Lang.Extensions;

namespace Common.Azure;

public class AzureApi : IAzureApi
{
    private readonly string _connectionString;

    public AzureApi(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IBlobContainer GetContainer(string containerName) => GetContainer(_connectionString, containerName);
    public IBlobContainer GetContainer(string connectionString, string containerName)
    {
        var offset = containerName.IndexOf('/').Let(it => it > -1 ? it : containerName.Length);
        var container = containerName.Substring(0, offset);
        var directory = containerName.Substring(offset, containerName.Length - offset);
        return new BlobContainer(new BlobContainerClient(connectionString, container), directory);
    }

    public IQueue GetQueue(string queueName) => GetQueue(_connectionString, queueName);
    public IQueue GetQueue(string connectionString, string queueName)
    {
        return new Queue(connectionString, queueName);
    }

    public IServiceBus GetServiceBus(string queueName) => GetServiceBus(_connectionString, queueName);
    public IServiceBus GetServiceBus(string connectionString, string queueName)
    {
        return new ServiceBus(connectionString, queueName);
    }

    public ITableEntities GetTableEntities(string tableName) => GetTableEntities(_connectionString, tableName);
    public ITableEntities GetTableEntities(string connectionString, string tableName)
    {
        return new TableEntities(new TableClient(connectionString, tableName));
    }
}