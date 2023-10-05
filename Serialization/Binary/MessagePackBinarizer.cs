// using MemoryPack;
using MessagePack;
using MessagePack.NodaTime;
using MessagePack.Resolvers;

namespace Common.Serialization.Binary;

public class MessagePackBinarizer : IBinarizer
{
    public static readonly MessagePackBinarizer Instance = new();

    private static readonly MessagePackSerializerOptions _options = MessagePackSerializerOptions.Standard.WithResolver(
        MessagePack.Resolvers.CompositeResolver.Create(
            NodatimeResolver.Instance,
            DynamicObjectResolverAllowPrivate.Instance,
            StandardResolverAllowPrivate.Instance
        )
    );

    public T Deserialize<T>(Span<byte> obj)
    {
        return MessagePackSerializer.Deserialize<T>(obj.ToArray(), _options) ?? throw new Exception($"Unable to unparcel into {typeof(T)}");
    }

    public T Deserialize<T>(Span<byte> obj, Type type)
    {
        return (T)(MessagePackSerializer.Deserialize(type, obj.ToArray(), _options) ?? throw new Exception($"Unable to unparcel into {type}"));
    }

    public async Task<T> Deserialize<T>(Stream input, CancellationToken token = default)
    {
        var result = await MessagePackSerializer.DeserializeAsync<T>(input, _options, token);

        if (result != null)
            return result;

        throw new Exception($"Unable to unparcel into {typeof(T)}");
    }

    public async Task<T> Deserialize<T>(Stream input, Type type, CancellationToken token = default)
    {
        var result = await MessagePackSerializer.DeserializeAsync(type, input, _options, token);

        if (result != null)
            return (T)result;

        throw new Exception($"Unable to unparcel into {type}");
    }
    
    public Span<byte> Serialize<T>(T obj)
    {
        return MessagePackSerializer.Typeless.Serialize(obj, _options);
    }

    public Span<byte> Serialize<T>(T obj, Type type)
    {
        return MessagePackSerializer.Typeless.Serialize(obj, _options);
    }

    public async Task Serialize<T>(T obj, Stream output, Type type, CancellationToken token = default)
    {
        await MessagePackSerializer.Typeless.SerializeAsync(output, obj, _options, token);
    }

    public async Task Serialize<T>(T obj, Stream output, CancellationToken token = default)
    {
        await MessagePackSerializer.Typeless.SerializeAsync(output, obj, _options, token);
    }
}