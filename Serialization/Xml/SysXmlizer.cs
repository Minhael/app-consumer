namespace Common.Serialization.Xml;

public sealed class SysXmlizer : IXmlizer
{
    public static readonly SysXmlizer Instance = new();

    public T Deserialize<T>(string obj)
    {
        throw new NotImplementedException();
    }

    public T Deserialize<T>(string obj, Type type)
    {
        throw new NotImplementedException();
    }

    public Task<T> Deserialize<T>(Stream input, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> Deserialize<T>(Stream input, Type type, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public string Serialize<T>(T obj)
    {
        throw new NotImplementedException();
    }

    public Task Serialize<T>(T obj, Stream output, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task Serialize<T>(T obj, Stream output, Type type, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}