using Microsoft.Extensions.DependencyInjection;

namespace Common.Lang.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection BindTransient<T, S>(this IServiceCollection self, Func<S, T>? factory = null) where S : T where T : class
    {
        return self.AddTransient<T>(s => s.GetRequiredService<S>().Let(x => factory?.Invoke(x) ?? x));
    }

    public static IServiceCollection BindScoped<T, S>(this IServiceCollection self, Func<S, T>? factory = null) where S : T where T : class
    {
        return self.AddScoped<T>(s => s.GetRequiredService<S>().Let(x => factory?.Invoke(x) ?? x));
    }

    public static IServiceCollection BindSingleton<T, S>(this IServiceCollection self, Func<S, T>? factory = null) where S : T where T : class
    {
        return self.AddSingleton<T>(s => s.GetRequiredService<S>().Let(x => factory?.Invoke(x) ?? x));
    }
}