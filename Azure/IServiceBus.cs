using Azure.Messaging.ServiceBus;
using Common.Serialization.Json;

namespace Common.Azure;

public interface IServiceBus
{
    Task SendMessage(object contents, CancellationToken token = default);
    Task SendMessage(IEnumerable<object> contents, CancellationToken token = default);
}

class ServiceBus : IServiceBus
{
    private readonly string _connStr;
    private readonly string _queue;

    public ServiceBus(string connStr, string queue)
    {
        _connStr = connStr;
        _queue = queue;
    }

    public async Task SendMessage(object contents, CancellationToken token = default)
    {
        await using var client = new ServiceBusClient(_connStr);
        await using var sender = client.CreateSender(_queue);
        await sender.SendMessageAsync(new ServiceBusMessage(contents.ComposeJson()), token);
    }

    public async Task SendMessage(IEnumerable<object> contents, CancellationToken token = default)
    {
        await using var client = new ServiceBusClient(_connStr);
        await using var sender = client.CreateSender(_queue);

        var payloads = contents.Select(x => x.ComposeJson()).ToList();
        
        while (payloads.Any())
        {
            using var batch = await sender.CreateMessageBatchAsync(token);
            payloads = payloads.Where(x => !batch.TryAddMessage(new ServiceBusMessage(x))).ToList();
            await sender.SendMessagesAsync(batch, token);
        }
    }
}