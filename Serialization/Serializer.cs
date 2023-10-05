namespace Common.Serialization
{
    public interface ISerializer
    {
        Task Serialize<T>(T obj, Stream output, Type type, CancellationToken token = default);
        Task Serialize<T>(T obj, Stream output, CancellationToken token = default);
        Task<T> Deserialize<T>(Stream input, CancellationToken token = default);
        Task<T> Deserialize<T>(Stream input, Type type, CancellationToken token = default);
    }
}