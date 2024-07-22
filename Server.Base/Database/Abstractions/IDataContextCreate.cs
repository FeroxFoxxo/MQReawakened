using Microsoft.Extensions.DependencyInjection;

namespace Server.Base.Database.Abstractions;
public interface IDataContextCreate : IDataContextInitialize
{
    public static abstract void AddContextToServiceProvider(
        IServiceCollection serviceCollection
    );
}
