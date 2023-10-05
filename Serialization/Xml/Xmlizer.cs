namespace Common.Serialization.Xml;

public interface IXmlizer : ISerializer
{
    string Serialize<T>(T obj);
    T Deserialize<T>(string obj);
    T Deserialize<T>(string obj, Type type);
}