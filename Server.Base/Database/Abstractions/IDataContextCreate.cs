using Microsoft.Extensions.DependencyInjection;

namespace Server.Base.Database.Abstractions;
public interface IDataContextCreate : IDataContextInitialize
{
    abstract static void AddContextToServiceProvider(
        IServiceCollection serviceCollection
    );
}
