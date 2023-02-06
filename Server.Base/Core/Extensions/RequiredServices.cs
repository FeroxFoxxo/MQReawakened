using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Module = Server.Base.Core.Abstractions.Module;

namespace Server.Base.Core.Extensions;

public static class RequiredServices
{
    public static IEnumerable<T> GetRequiredServices<T>(this IServiceProvider services, IEnumerable<Module> modules)
        where T : class => GetServices<T>(modules).Select(t => services.GetRequiredService(t) as T);

    public static IEnumerable<Type> GetServices<T>(this IEnumerable<Module> modules) =>
        modules.Select(m => m.GetType().Assembly)
            .SelectMany(a =>
                a.GetServices<T>()
            );

    public static IEnumerable<Type> GetServices<T>(this Assembly assembly) =>
        assembly.GetTypes().Where(
            t => typeof(T).IsAssignableFrom(t) &&
                 !t.IsInterface &&
                 !t.IsAbstract
        );
}
