using System.Collections.Immutable;

namespace Common.Lang.Collections;

public static class EmptyCollections
{
    public static IDictionary<T, V> Dict<T, V>() where T : notnull
    {
        return ImmutableDictionary<T, V>.Empty;
    }
}