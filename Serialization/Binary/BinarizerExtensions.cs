using Common.Serialization.Binary;

namespace Common.Serialization.Json;

public static class BinarizerExtensions
{
    public readonly static IBinarizer Default = MemoryPackBinarizer.Instance;

    public static T Deserialize<T>(this Span<byte> self, IBinarizer? parser = null)
    {
        if (parser == null)
            parser = Default;

        return parser.Deserialize<T>(self);
    }

    public static T Deserialize<T>(this Span<byte> self, Type type, IBinarizer? parser = null)
    {
        if (parser == null)
            parser = Default;

        return parser.Deserialize<T>(self, type);
    }

    public static Task<T> Deserialize<T>(this Stream self, IBinarizer? parser = null, CancellationToken token = default)
    {
        if (parser == null)
            parser = Default;

        return parser.Deserialize<T>(self, token);
    }

    public static Task<T> Deserialize<T>(this Stream self, Type type, IBinarizer? parser = null, CancellationToken token = default)
    {
        if (parser == null)
            parser = Default;

        return parser.Deserialize<T>(self, type, token);
    }

    public static Span<byte> Serialize<T>(this T self, IBinarizer? composer = null)
    {
        if (composer == null)
            composer = Default;

        return composer.Serialize(self);
    }

    public static Span<byte> Serialize<T>(this T self, Type type, IBinarizer? composer = null)
    {
        if (composer == null)
            composer = Default;

        return composer.Serialize(self, type);
    }

    public static Task Serialize<T>(this T self, Stream output, IBinarizer? composer = null, CancellationToken token = default)
    {
        if (composer == null)
            composer = Default;

        return composer.Serialize(self, output, token);
    }

    public static Task Serialize<T>(this T self, Stream output, Type type, IBinarizer? composer = null, CancellationToken token = default)
    {
        if (composer == null)
            composer = Default;

        return composer.Serialize(self, output, type, token);
    }
}