using Microsoft.Extensions.DependencyInjection;

namespace Common.Misc;

//  https://stackoverflow.com/questions/44934511/does-net-core-dependency-injection-support-lazyt
class Lazier<T> : Lazy<T> where T : notnull
{
    public Lazier(IServiceProvider provider)
        : base(() => provider.GetRequiredService<T>())
    {
    }
}

public static class LazierExtensions
{
    public static void AddLazies(this IServiceCollection self)
    {
        self.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
    }
}