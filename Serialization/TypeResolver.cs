using Common.Lang.Extensions;

namespace Common.Serialization;

public class TypeResolver
{
    protected virtual string GetName(Type type) => type.Name;

    private readonly IDictionary<string, Type> _types;
    private readonly IDictionary<Type, string> _names;

    public TypeResolver(IEnumerable<Type> types)
    {
        _types = types.ToDictionary(x => GetName(x), x => x);
        _names = types.ToDictionary(x => x, x => GetName(x));
    }

    public Type? Lookup(string name)
    {
        return Type.GetType(name) ?? _types.GetOrDefault(name);
    }

    public string ReverseLookup(Type type)
    {
        return _names.GetOrDefault(type) ?? GetName(type);
    }
}