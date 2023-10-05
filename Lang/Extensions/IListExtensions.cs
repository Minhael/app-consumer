
using System.Diagnostics;

namespace Common.Lang.Extensions;

[DebuggerStepThrough]
public static class IListExtensions
{
    public static void AddAll<T>(this IList<T> self, params T[] others)
    {
        self.AddAll(others.AsEnumerable());
    }

    public static void AddAll<T>(this IList<T> self, IEnumerable<T> others)
    {
        foreach (var t in others)
            self.Add(t);
    }
}