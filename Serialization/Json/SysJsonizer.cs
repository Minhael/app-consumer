using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Lang.Extensions;
using Common.Telemetry;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.Text;
using OpenTelemetry.Trace;

namespace Common.Serialization.Json;

public class SysJsonizer : IJsonizer
{
    public static readonly SysJsonizer Instance = new();

    public static void ConfigureDefault(JsonSerializerOptions options)
    {
        //  Order matters. Precise converter first.

        //  NodaTime custom
        // Options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        options.Converters.Add(InstantConverter.Instance);
        options.Converters.Add(NodaConverters.IntervalConverter);
        options.Converters.Add(NodaConverters.LocalDateConverter);
        options.Converters.Add(NodaConverters.LocalDateTimeConverter);
        options.Converters.Add(NodaConverters.LocalTimeConverter);
        options.Converters.Add(NodaConverters.AnnualDateConverter);
        options.Converters.Add(NodaConverters.DateIntervalConverter);
        options.Converters.Add(NodaConverters.OffsetConverter);
        options.Converters.Add(NodaConverters.CreateDateTimeZoneConverter(DateTimeZoneProviders.Tzdb));
        options.Converters.Add(NodaConverters.DurationConverter);
        options.Converters.Add(NodaConverters.RoundtripPeriodConverter);
        options.Converters.Add(new NodaPatternConverter<OffsetDateTime>(OffsetDateTimePattern.ExtendedIso));
        options.Converters.Add(NodaConverters.OffsetDateConverter);
        options.Converters.Add(NodaConverters.OffsetTimeConverter);
        options.Converters.Add(new NodaPatternConverter<ZonedDateTime>(ZonedDateTimePattern.ExtendedFormatOnlyIso.WithZoneProvider(DateTimeZoneProviders.Tzdb)));

        options.ConfigureDateTimePattern();
        options.ConfigureForNewtonsoftCompatible();
    }

    private static readonly Tracer _tracer = Measure.CreateTracer<SysJsonizer>();

    private JsonSerializerOptions Options { get; init; }

    public SysJsonizer()
    {
        Options = new JsonSerializerOptions();
        ConfigureDefault(Options);
    }

    public SysJsonizer(JsonSerializerOptions options)
    {
        Options = options;
    }

    public T Deserialize<T>(string obj)
    {
        using var span = _tracer.StartActiveSpan($"deserialize ${typeof(T).Name}", SpanKind.Internal);
        return JsonSerializer.Deserialize<T>(obj, Options) ?? throw new JsonException($"Unable to deserialize as {typeof(T)}");
    }

    public T Deserialize<T>(string obj, Type type)
    {
        if (type != typeof(T) && !typeof(T).IsAssignableFrom(type)) throw new JsonException($"{type} is not a {typeof(T)}");
        using var span = _tracer.StartActiveSpan($"deserialize {type.Name}", SpanKind.Internal);
        var rt = JsonSerializer.Deserialize(obj, type, Options) ?? throw new JsonException($"Unable to deserialize as {type}");
        return rt.SafeCast<T>() ?? throw new JsonException($"{rt} is not a {typeof(T)}");
    }

    public async Task<T> Deserialize<T>(Stream input, CancellationToken token = default)
    {
        using var span = _tracer.StartActiveSpan($"deserialize {typeof(T).Name}", SpanKind.Internal);
        return await JsonSerializer.DeserializeAsync<T>(input, Options, token) ?? throw new JsonException($"Unable to deserialize as {typeof(T)}");
    }

    public async Task<T> Deserialize<T>(Stream input, Type type, CancellationToken token = default)
    {
        if (type != typeof(T) && !typeof(T).IsAssignableFrom(type)) throw new JsonException($"{type} is not a {typeof(T)}");
        using var span = _tracer.StartActiveSpan($"deserialize {type.Name}", SpanKind.Internal);
        var rt = await JsonSerializer.DeserializeAsync(input, type, Options, token) ?? throw new JsonException($"Cannot deserialize to {type}");
        return rt.SafeCast<T>() ?? throw new JsonException($"{rt} is not a {typeof(T)}");
    }

    public string Serialize<T>(T obj)
    {
        using var span = _tracer.StartActiveSpan($"serialize {typeof(T).Name}", SpanKind.Internal);
        return JsonSerializer.Serialize(obj, Options);
    }

    public async Task Serialize<T>(T obj, Stream output, CancellationToken token = default)
    {
        using var span = _tracer.StartActiveSpan($"serialize {typeof(T).Name}", SpanKind.Internal);
        await JsonSerializer.SerializeAsync(output, obj, Options, token);
    }

    public string Serialize<T>(T obj, Type type)
    {
        using var span = _tracer.StartActiveSpan($"serialize {type.Name}", SpanKind.Internal);
        return JsonSerializer.Serialize(obj, type, Options);
    }

    public async Task Serialize<T>(T obj, Stream output, Type type, CancellationToken token = default)
    {
        using var span = _tracer.StartActiveSpan($"serialize {type.Name}", SpanKind.Internal);
        await JsonSerializer.SerializeAsync(output, obj, type, Options, token);
    }
}

public static class SysJsonizerExtensions
{
    public static void ConfigureForNewtonsoftCompatible(this JsonSerializerOptions options)
    {
        options.AllowTrailingCommas = true;
        options.IncludeFields = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.Converters.AddAll(
            new SomeTypesBooleanConverter(),
            new SomeTypesIntConverter(),
            new SomeTypesLongConverter(),
            new AnyTypesStringConverter(),
            new DictionaryTKeyTValueConverterFactory(),
            new AnnoymousObjectConverter.Factory(),
            new CastJsonConverterFactory(),
            new TypeConverterJsonConverterFactory()
        );
    }

    public static void ConfigureDateTimePattern(this JsonSerializerOptions options, string pattern = DateTimeConverter.GeneralISO)
    {
        options.Converters.Add(new DateTimeConverter(pattern));
    }
}

//  This allows parsing json to object but with closest-match primitive types
class AnnoymousObjectConverter : JsonConverter<object>
{

    public class Factory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(object) == typeToConvert;
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return new AnnoymousObjectConverter();
        }
    }


    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.True => reader.GetBoolean(),
            JsonTokenType.False => reader.GetBoolean(),
            JsonTokenType.Number => reader.GetByteOrDefault() ?? reader.GetShortOrDefault() ?? reader.GetIntOrDefault() ?? reader.GetLongOrDefault() ?? reader.GetDecimalOrDefault(),
            JsonTokenType.String => (object?)reader.GetGuidOrDefault() ?? reader.GetString(),
            JsonTokenType.PropertyName => reader.GetString(),
            JsonTokenType.StartArray => ReadArray(ref reader, options),
            JsonTokenType.StartObject => ReadObject(ref reader, options),
            _ => throw new JsonException($"Unexpected token {reader.TokenType} at {reader.TokenStartIndex}"),
        };
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }

    private List<object> ReadArray(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var list = new List<object>();
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndArray:
                    return list;
                default:
                    var item = Read(ref reader, typeof(object), options);
                    if (item != null) list.Add(item);
                    break;
            }
        }
        throw new JsonException($"Invalid JSON? Never reached end of array");
    }

    private ExpandoObject ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var obj = new ExpandoObject();
        var dict = obj as IDictionary<string, object>;
        string? key = null;
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndObject:
                    return obj;
                case JsonTokenType.PropertyName:
                    key = reader.GetString();
                    break;
                default:
                    var value = Read(ref reader, typeof(object), options);
                    if (key != null && value != null) dict.Add(key, value);
                    break;
            }
        }
        throw new JsonException($"Invalid JSON? Never reached end of object");
    }
}

static class JsonReaderExtension
{
    public static byte? GetByteOrDefault(this ref Utf8JsonReader self) => self.TryGetByte(out byte rt) ? rt : null;
    public static decimal? GetDecimalOrDefault(this ref Utf8JsonReader self) => self.TryGetDecimal(out decimal rt) ? rt : null;
    public static Guid? GetGuidOrDefault(this ref Utf8JsonReader self) => self.TryGetGuid(out Guid rt) ? rt : null;
    public static short? GetShortOrDefault(this ref Utf8JsonReader self) => self.TryGetInt16(out short rt) ? rt : null;
    public static int? GetIntOrDefault(this ref Utf8JsonReader self) => self.TryGetInt32(out int rt) ? rt : null;
    public static long? GetLongOrDefault(this ref Utf8JsonReader self) => self.TryGetInt64(out long rt) ? rt : null;
}

//  https://github.com/dotnet/runtime/issues/1761
//  This allows parsing values with TypeConverter defined for the types
class TypeConverterJsonConverterFactory : JsonConverterFactory
{
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeConverter = TypeDescriptor.GetConverter(typeToConvert);
        var jsonConverter = (JsonConverter?)Activator.CreateInstance(typeof(TypeConverterJsonConverter<>).MakeGenericType(new Type[] { typeToConvert }), new object[] { typeConverter, options });
        return jsonConverter;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetCustomAttributes<TypeConverterAttribute>(inherit: true).Any();
    }

    private sealed class TypeConverterJsonConverter<T> : JsonConverter<T>
    {
        private readonly TypeConverter _typeConverter;

        public TypeConverterJsonConverter(TypeConverter tc, JsonSerializerOptions options)
        {
            _typeConverter = tc;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetLongOrDefault()?.ToString() ?? reader.GetDecimalOrDefault()?.ToString(),
                JsonTokenType.True => reader.GetBoolean().ToString(),
                JsonTokenType.False => reader.GetBoolean().ToString(),
                JsonTokenType.PropertyName => reader.GetString(),
                _ => null
            };
            if (value != null)
                return (T?)_typeConverter.ConvertFromInvariantString(value);
            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value != null && _typeConverter.ConvertToInvariantString(value) is { } x)
            {
                writer.WriteStringValue(x);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

//  This allows parsing values by type casting automatically
class CastJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return GetInwardCast(typeToConvert) != null || GetOutwardCast(typeToConvert) != null;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter?)Activator.CreateInstance(typeof(Converter<>).MakeGenericType(new Type[] { typeToConvert }), new object?[] { GetInwardCast(typeToConvert), GetOutwardCast(typeToConvert), options });
    }

    private static MethodInfo? GetInwardCast(Type targetType)
    {
        return GetCastMethod(targetType, typeof(string), targetType);
    }

    private static MethodInfo? GetOutwardCast(Type targetType)
    {
        return GetCastMethod(targetType, targetType, typeof(string));
    }

    private static MethodInfo? GetCastMethod(Type typeToConvert, Type from, Type to)
    {
        return typeToConvert.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod)
            .Where(mi => (mi.Name == "op_Implicit" || mi.Name == "op_Explicit") && mi.ReturnType == to)
            .FirstOrDefault(mi =>
            {
                var pi = mi.GetParameters().FirstOrDefault();
                return pi != null && pi.ParameterType == from;
            });
    }

    private sealed class Converter<T> : JsonConverter<T>
    {
        private readonly MethodInfo? _inward;
        private readonly MethodInfo? _outward;

        public Converter(MethodInfo? inward, MethodInfo? outward, JsonSerializerOptions options)
        {
            _inward = inward;
            _outward = outward;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.TokenType switch
            {
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetLongOrDefault()?.ToString() ?? reader.GetDecimalOrDefault()?.ToString(),
                JsonTokenType.True => reader.GetBoolean().ToString(),
                JsonTokenType.False => reader.GetBoolean().ToString(),
                JsonTokenType.PropertyName => reader.GetString(),
                _ => null
            };
            if (value != null)
                return (T?)_inward?.Invoke(null, new object[] { value });
            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteStringValue((string?)_outward?.Invoke(null, new object[] { value }) ?? value.ToString());
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}

//  https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-converters-how-to?pivots=dotnet-6-0
//  This allows parsing json objects as IDictionary with key of any types
public class DictionaryTKeyTValueConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }

        if (typeToConvert.GetGenericTypeDefinition() != typeof(Dictionary<,>) && typeToConvert.GetGenericTypeDefinition() != typeof(IDictionary<,>))
        {
            return false;
        }

        if (typeToConvert.GetGenericArguments()[0] == typeof(string))
            return false;

        return true;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var keyType = typeToConvert.GetGenericArguments()[0];
        var valueType = typeToConvert.GetGenericArguments()[1];
        var containerType = typeToConvert.GetGenericTypeDefinition() switch
        {
            var t when t == typeof(Dictionary<,>) => typeof(DictionaryConverter<,>),
            _ => typeof(IDictionaryConverter<,>)
        };

        JsonConverter converter = (JsonConverter)Activator.CreateInstance(
            containerType.MakeGenericType(new Type[] { keyType, valueType }),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: new object[] { options },
            culture: null)!;

        return converter;
    }

    private sealed class DictionaryConverter<TKey, TValue> : Converter<Dictionary<TKey, TValue>, TKey, TValue> where TKey : notnull
    {
        protected override Dictionary<TKey, TValue> CreateContainer() => new();
        public DictionaryConverter(JsonSerializerOptions options) : base(options)
        {
        }
    }

    private sealed class IDictionaryConverter<TKey, TValue> : Converter<IDictionary<TKey, TValue>, TKey, TValue> where TKey : notnull
    {
        protected override IDictionary<TKey, TValue> CreateContainer() => new Dictionary<TKey, TValue>();
        public IDictionaryConverter(JsonSerializerOptions options) : base(options)
        {
        }
    }

    private abstract class Converter<TType, TKey, TValue> : JsonConverter<TType> where TKey : notnull where TType : class, IDictionary<TKey, TValue>
    {
        protected abstract TType CreateContainer();
        private readonly JsonConverter<TKey> _keyConverter;
        private readonly JsonConverter<TValue> _valueConverter;
        private readonly Type _keyType;
        private readonly Type _valueType;

        protected Converter(JsonSerializerOptions options)
        {
            // For performance, use the existing converter if available.
            _keyConverter = (JsonConverter<TKey>)options.GetConverter(typeof(TKey));
            _valueConverter = (JsonConverter<TValue>)options.GetConverter(typeof(TValue));

            // Cache the key and value types.
            _keyType = typeof(TKey);
            _valueType = typeof(TValue);
        }

        public override TType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = CreateContainer();
            TKey? key = default;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject:
                        return dictionary;
                    case JsonTokenType.PropertyName:
                        key = _keyConverter.Read(ref reader, _keyType, options);
                        break;
                    default:
                        var value = _valueConverter.Read(ref reader, _valueType, options);
                        if (key != null && value != null) dictionary.Add(key, value);
                        break;
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, TType value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<TKey, TValue> entry in value)
            {
                var propertyName = entry.Key.ToString();

                if (propertyName != null)
                {
                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);
                    _valueConverter.Write(writer, entry.Value, options);
                }
            }

            writer.WriteEndObject();
        }
    }
}

//  This allows parsing any JSON types to string
public class AnyTypesStringConverter : JsonConverter<string>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(string);
    }

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.True => reader.GetBoolean().ToString(),
            JsonTokenType.False => reader.GetBoolean().ToString(),
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetLongOrDefault()?.ToString() ?? reader.GetDecimalOrDefault()?.ToString(),
            JsonTokenType.PropertyName => reader.GetString(),
            JsonTokenType.StartArray => JsonSerializer.Serialize(JsonSerializer.Deserialize(ref reader, typeof(object[]), options), options),
            JsonTokenType.StartObject => JsonSerializer.Serialize(JsonSerializer.Deserialize(ref reader, typeof(Dictionary<string, object>), options), options),
            _ => null
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

//  This allows parsing some JSON types to boolean
public class SomeTypesBooleanConverter : JsonConverter<bool>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(bool);
    }

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => false,
            JsonTokenType.True => reader.GetBoolean(),
            JsonTokenType.False => reader.GetBoolean(),
            JsonTokenType.String => reader.GetString()?.Let(x => x.ToLower().Equals("true")) ?? false,
            JsonTokenType.Number => reader.GetLongOrDefault()?.Let(x => x != 0) ?? reader.GetDecimalOrDefault()?.Let(x => x != 0) ?? false,
            _ => false
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}

//  This allows parsing some JSON types to int
public class SomeTypesIntConverter : JsonConverter<int>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(int);
    }

    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => 0,
            JsonTokenType.True => 1,
            JsonTokenType.False => 0,
            JsonTokenType.String => reader.GetString()?.Let(x => Int32.TryParse(x, out int rt) ? rt : 0) ?? 0,
            JsonTokenType.Number => reader.GetLongOrDefault()?.Let(x => Convert.ToInt32(x)) ?? reader.GetDecimalOrDefault()?.Let(x => Convert.ToInt32(x)) ?? 0,
            _ => 0
        };
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

//  This allows parsing some JSON types to long
public class SomeTypesLongConverter : JsonConverter<long>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(long);
    }

    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => 0,
            JsonTokenType.True => 1,
            JsonTokenType.False => 0,
            JsonTokenType.String => reader.GetString()?.Let(x => Int64.TryParse(x, out long rt) ? rt : 0) ?? 0,
            JsonTokenType.Number => reader.GetLongOrDefault()?.Let(x => Convert.ToInt64(x)) ?? reader.GetDecimalOrDefault()?.Let(x => Convert.ToInt64(x)) ?? 0,
            _ => 0
        };
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

//  This defaults dotnet DateTime objects are UTC datetime. Use Nodatime LocalDateTime otherwise.
//  https://docs.microsoft.com/en-us/dotnet/standard/datetime/system-text-json-support
//  https://stackoverflow.com/questions/58102189/formatting-datetime-in-asp-net-core-3-0-using-system-text-json
public class DateTimeConverter : JsonConverter<DateTime>
{
    internal const string GeneralISO = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";
    internal const string ExtendedISO = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffZ";

    private readonly string _pattern;

    public DateTimeConverter(string pattern = GeneralISO)
    {
        _pattern = pattern;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.Let(x => DateTime.Parse(x)) ?? default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        switch (value.Kind)
        {
            case DateTimeKind.Local:
                writer.WriteStringValue(value.ToUniversalTime().ToString(_pattern));
                break;
            default:
                writer.WriteStringValue(value.ToString(_pattern));
                break;
        }
    }
}

public sealed class InstantConverter : JsonConverter<Instant>
{
    public static InstantConverter Instance = new InstantConverter();
    private static readonly JsonConverter<Instant> _writer = new NodaPatternConverter<Instant>(InstantPattern.General);
    private static readonly JsonConverter<Instant> _parser = new NodaPatternConverter<Instant>(InstantPattern.ExtendedIso);

    public override Instant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return _parser.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options)
    {
        _writer.Write(writer, value, options);
    }
}