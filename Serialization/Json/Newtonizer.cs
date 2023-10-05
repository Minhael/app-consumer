using Common.Lang.Extensions;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Common.Serialization.Json
{

    public class Newtonizer : IJsonizer
    {
        static Newtonizer()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        }

        public static readonly Newtonizer Instance = new();

        public JsonSerializerSettings Settings { get; init; }
        private readonly JsonSerializer _serializer;

        public Newtonizer()
        {
            Settings = new JsonSerializerSettings();
            _serializer = JsonSerializer.Create(Settings);
        }

        public Newtonizer(JsonSerializerSettings settings)
        {
            Settings = settings;
            _serializer = JsonSerializer.Create(settings);
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public string Serialize<T>(T obj, Type type)
        {
            return JsonConvert.SerializeObject(obj, type, Settings);
        }

        public Task Serialize<T>(T obj, Stream output, CancellationToken token = default)
        {
            var w = new JsonTextWriter(new StreamWriter(output));
            _serializer.Serialize(w, obj);
            w.Flush();
            output.Seek(0, SeekOrigin.Begin);
            return Task.CompletedTask;
        }

        public Task Serialize<T>(T obj, Stream output, Type type, CancellationToken token = default)
        {
            var w = new JsonTextWriter(new StreamWriter(output));
            _serializer.Serialize(w, obj, type);
            w.Flush();
            output.Seek(0, SeekOrigin.Begin);
            return Task.CompletedTask;
        }

        public T Deserialize<T>(string obj)
        {
            var rt = JsonConvert.DeserializeObject<T>(obj, Settings);

            if (rt != null)
                return rt;

            throw new JsonException();
        }

        public T Deserialize<T>(string obj, Type type)
        {
            if (type != typeof(T) && !typeof(T).IsAssignableFrom(type)) throw new JsonException($"{type} is not a {typeof(T)}");
            var rt = JsonConvert.DeserializeObject(obj, type) ?? throw new JsonException($"Cannot deserialize to {type}");
            return rt.SafeCast<T>() ?? throw new JsonException($"{rt} is not a {typeof(T)}");
        }

        //  TODO    Now JSON.NET is not working well with async. Fix when this issue closes
        //  https://github.com/JamesNK/Newtonsoft.Json/issues/1193
        public Task<T> Deserialize<T>(Stream input, Type type, CancellationToken token = default)
        {
            if (type != typeof(T) && !typeof(T).IsAssignableFrom(type)) throw new JsonException($"{type} is not a {typeof(T)}");
            var r = new JsonTextReader(new StreamReader(input));
            var rt = _serializer.Deserialize(r, type) ?? throw new JsonException($"Cannot deserialize to {type}");
            return Task.FromResult(rt.SafeCast<T>() ?? throw new JsonException($"{rt} is not a {typeof(T)}"));
        }

        public Task<T> Deserialize<T>(Stream input, CancellationToken token = default)
        {
            var r = new JsonTextReader(new StreamReader(input));
            var rt = _serializer.Deserialize<T>(r) ?? throw new JsonException();
            return Task.FromResult(rt);
        }
    }
}