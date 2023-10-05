namespace Common.Lang.Collections;

/// <summary>
/// String comparator that return true when prefix of the string x are the same to string y.
/// </summary>
public class StartWithComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        return y != null && x?.StartsWith(y) == true;
    }

    public int GetHashCode(string obj)
    {
        return obj.GetHashCode();
    }
}