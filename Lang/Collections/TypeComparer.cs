using Common.Lang.Extensions;

namespace Common.Lang.Collections;

public class TypeComparer : EqualityComparer<Type>
{
    public override bool Equals(Type? x, Type? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        if (x.IsGenericType && y.IsGenericType) return CompareGenericType(x, y);
        return x == y;
    }

    public override int GetHashCode(Type obj)
    {
        if (obj.IsGenericType)
            return GetGenericHashCode(obj);
        return obj.GetHashCode();
    }

    private bool CompareGenericType(Type x, Type y)
    {
        if (x.GetGenericTypeDefinition() != y.GetGenericTypeDefinition()) return false;
        if (x.GetGenericArguments().Length != y.GetGenericArguments().Length) return false;
        return x.GetGenericArguments().Zip(y.GetGenericArguments(), (a, b) => (a, b)).All(x => Equals(x.a, x.b));
    }

    private int GetGenericHashCode(Type obj)
    {
        //  https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        var str = GetGenericTypeExpandedClassString(obj);
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    private string GetGenericTypeExpandedClassString(Type obj)
    {
        if (obj.IsGenericType)
            return $"{obj.GetGenericTypeDefinition().Name}_${obj.GetGenericArguments().Select(x => GetGenericTypeExpandedClassString(x)).JoinString()}";
        return $"{obj.Name}";
    }
}