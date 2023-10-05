namespace Common.Serialization.Binary;

public interface IBinarizer : ISerializer
{
    Span<byte> Serialize<T>(T obj);
    Span<byte> Serialize<T>(T obj, Type type);
    T Deserialize<T>(Span<byte> obj);
    T Deserialize<T>(Span<byte> obj, Type type);
}