namespace Common.Messenger
{
    public interface IProducer
    {
        Task Publish(Message message, CancellationToken token = default);
    }
}