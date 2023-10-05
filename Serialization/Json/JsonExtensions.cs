using Common.Lang.Extensions;

namespace Common.Serialization.Json;

public static class JsonExtensions
{
    public readonly static IJsonizer Default = SysJsonizer.Instance;

    public static T ParseJsonAs<T>(this string self, IJsonizer? parser = null)
    {
        if (parser == null)
            parser = Default;

        return parser.Deserialize<T>(self);
    }

    public static T ParseJsonAs<T>(this string self, Type type, IJsonizer? parser = null)
    {
        if (parser == null)
            parser = Default;

        return parser.Deserialize<T>(self, type);
    }

    public static Task<T> ParseJsonAs<T>(this Stream self, IJsonizer? parser = null, CancellationToken token = default)
    {
        if (parser == null)
            parser = Default;

        return parser.Deserialize<T>(self, token);
    }

    public static Task<T> ParseJsonAs<T>(this Stream self, Type type, IJsonizer? parser = null, CancellationToken token = default)
    {
        if (parser == null)
            parser = Default;

        return parser.Deserialize<T>(self, type, token);
    }

    public static T ParseJsonAs<T>(this byte[] self, IJsonizer? parser = null) => self.Utf8().ParseJsonAs<T>(parser);
    public static T ParseJsonAs<T>(this byte[] self, Type type, IJsonizer? parser = null, CancellationToken token = default) => self.Utf8().ParseJsonAs<T>(type, parser);

    public static string ComposeJson<T>(this T self, IJsonizer? composer = null)
    {
        if (composer == null)
            composer = Default;

        return composer.Serialize(self);
    }

    public static string ComposeJson<T>(this T self, Type type, IJsonizer? composer = null)
    {
        if (composer == null)
            composer = Default;

        return composer.Serialize(self, type);
    }

    public static Task ComposeJson<T>(this T self, Stream output, IJsonizer? composer = null, CancellationToken token = default)
    {
        if (composer == null)
            composer = Default;

        return composer.Serialize(self, output, token);
    }

    public static Task ComposeJson<T>(this T self, Stream output, Type type, IJsonizer? composer = null, CancellationToken token = default)
    {
        if (composer == null)
            composer = Default;

        return composer.Serialize(self, output, type, token);
    }

    public static byte[] ComposeJsonBytes<T>(this T self, IJsonizer? composer = null) => self.ComposeJson(composer).Utf8();
    public static byte[] ComposeJsonBytes<T>(this T self, Type type, IJsonizer? composer = null) => self.ComposeJson(type, composer).Utf8();
}