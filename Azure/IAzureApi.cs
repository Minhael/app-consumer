namespace Common.Azure;

public interface IAzureApi
{
    ITableEntities GetTableEntities(string tableName);
    ITableEntities GetTableEntities(string connectionString, string tableName);
    IServiceBus GetServiceBus(string queueName);
    IServiceBus GetServiceBus(string connectionString, string queueName);
    IBlobContainer GetContainer(string containerName);
    IBlobContainer GetContainer(string connectionString, string containerName);
    IQueue GetQueue(string queueName);
    IQueue GetQueue(string connectionString, string queueName);
}