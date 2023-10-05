using System.Diagnostics;

namespace Common.Lang.Extensions;

[DebuggerStepThrough]
public static class WeakReferenceExtensions
{
    /// <summary>
    /// Get the referencing object T if not recycled; otherwise generate new object T, keep the reference and return the value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="generator"></param>
    /// <returns></returns>
    public static T GetOrPut<T>(this WeakReference<T> self, Func<T> generator) where T : class
    {
        if (self.TryGetTarget(out T? target))
            return target.NotNull();

        var instance = generator();
        self.SetTarget(instance);
        return instance;
    }
}