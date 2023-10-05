using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Lang.Extensions;

namespace Common.Data;

public class Normalized<T> where T : notnull
{
    public static Normalized<T> Create(DataProcessor processor, T value)
    {
        processor.Normalize(value);
        return new Normalized<T>(value);
    }

    public T Value { get; }

    public Normalized(T value)
    {
        Value = value;
    }
}

public class NormalizedJsonConverterFactory : JsonConverterFactory
{
    private readonly DataProcessor _processor;

    public NormalizedJsonConverterFactory(DataProcessor processor)
    {
        _processor = processor;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonConverter = (JsonConverter?)Activator.CreateInstance(typeof(NormalizedJsonConverter<>).MakeGenericType(new Type[] { typeToConvert.GetGenericArguments()[0] }), new object[] { _processor, options });
        return jsonConverter;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Normalized<>);
    }

    private sealed class NormalizedJsonConverter<T> : JsonConverter<Normalized<T>> where T : notnull
    {
        private readonly DataProcessor _processor;
        public NormalizedJsonConverter(DataProcessor processor, JsonSerializerOptions options)
        {
            _processor = processor;
        }

        public override Normalized<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = (T?)JsonSerializer.Deserialize(ref reader, typeof(T), options);
            return value?.Let(x => Normalized<T>.Create(_processor, x));
        }

        public override void Write(Utf8JsonWriter writer, Normalized<T> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}