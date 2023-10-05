namespace Common.Serialization.Json;

public interface IJsonizer : ISerializer
{
    string Serialize<T>(T obj);
    string Serialize<T>(T obj, Type type);
    T Deserialize<T>(string obj);
    T Deserialize<T>(string obj, Type type);
}