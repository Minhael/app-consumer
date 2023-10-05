using System.Dynamic;
using Common.Serialization.Json;
using ImpromptuInterface;

namespace Common.Lang.Extensions;

public static class DynamicExtensions
{
    public static dynamic MockInterface(this ExpandoObject obj, Type type, IJsonizer? jsonizer = null)
    {
        return FixExpandoObjectMembersType(obj, type, jsonizer).ActLike(type);
    }

    private static ExpandoObject FixExpandoObjectMembersType(ExpandoObject obj, Type type, IJsonizer? jsonizer)
    {
        var dict = obj as IDictionary<string, object?>;
        var lookup = new Dictionary<string, object?>(dict, StringComparer.OrdinalIgnoreCase);
        foreach (var prop in type.GetProperties().Where(x => lookup.ContainsKey(x.Name)))
            dict[prop.Name] = FixType(lookup[prop.Name], prop.PropertyType, jsonizer);
        foreach (var field in type.GetFields().Where(x => lookup.ContainsKey(x.Name)))
            dict[field.Name] = FixType(lookup[field.Name], field.FieldType, jsonizer);
        return obj;
    }

    private static object? FixType(object? origin, Type to, IJsonizer? jsonizer)
    {
        if (origin == null) return null;
        var from = origin.GetType();
        if (from == to) return origin;
        if (to.IsInterface && !to.IsGenericType && origin is ExpandoObject eo)
            return FixExpandoObjectMembersType(eo, to, jsonizer);
        if (from.IsPrimitive)
            return origin.ChangeType(to);
        return origin.ComposeJson(jsonizer).ParseJsonAs<object>(to, jsonizer);
    }
}