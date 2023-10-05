using MemoryPack;

namespace Common.Serialization.Binary;

public class MemoryPackBinarizer : IBinarizer
{
    public static readonly MemoryPackBinarizer Instance = new();

    public T Deserialize<T>(Span<byte> obj)
    {
        return MemoryPackSerializer.Deserialize<T>(obj) ?? throw new Exception($"Unable to unparcel into {typeof(T)}");
    }

    public T Deserialize<T>(Span<byte> obj, Type type)
    {
        return (T)(MemoryPackSerializer.Deserialize(type, obj) ?? throw new Exception($"Unable to unparcel into {type}"));
    }

    public async Task<T> Deserialize<T>(Stream input, CancellationToken token = default)
    {
        var result = await MemoryPackSerializer.DeserializeAsync<T>(input, null, token);

        if (result != null)
            return result;

        throw new Exception($"Unable to unparcel into {typeof(T)}");
    }

    public async Task<T> Deserialize<T>(Stream input, Type type, CancellationToken token = default)
    {
        var result = await MemoryPackSerializer.DeserializeAsync(type, input, null, token);

        if (result != null)
            return (T)result;

        throw new Exception($"Unable to unparcel into {type}");
    }

    public Span<byte> Serialize<T>(T obj)
    {
        return MemoryPackSerializer.Serialize<T>(obj);
    }

    public Span<byte> Serialize<T>(T obj, Type type)
    {
        return MemoryPackSerializer.Serialize(type, obj);
    }

    public async Task Serialize<T>(T obj, Stream output, Type type, CancellationToken token = default)
    {
        await MemoryPackSerializer.SerializeAsync(type, output, obj, null, token);
    }

    public async Task Serialize<T>(T obj, Stream output, CancellationToken token = default)
    {
        await MemoryPackSerializer.SerializeAsync<T>(output, obj, null, token);
    }
}