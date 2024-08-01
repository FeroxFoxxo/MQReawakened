using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Helpers;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Core.Workers;
using Server.Base.Database.Abstractions;
using Server.Base.Logging;
using Server.Base.Network.Helpers;
using Server.Base.Timers.Helpers;

namespace Server.Base;

public class Server(ILogger<Server> logger) : Module(logger)
{
    public override void AddDatabase(IServiceCollection services, Module[] modules)
    {
        Logger.LogDebug("Loading database locks");

        foreach (var service in modules.GetServices<DbLock>())
        {
            Logger.LogTrace("Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogInformation("Loaded database locks");

        Logger.LogDebug("Loading database contexts");

        foreach (var type in modules.GetServices<IDataContextInitialize>())
        {
            Logger.LogTrace("Loaded: {ServiceName}", type.Name);
            type.GetMethod("AddContextToServiceProvider")?.Invoke(null, [services]);
        }

        Logger.LogInformation("Loaded database contexts");
    }

    public override void AddLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddProvider(new LoggerProvider());
    }

    public override void AddServices(IServiceCollection services, Module[] modules)
    {
        Logger.LogDebug("Loading hosted services");

        {
            services.AddHostedService<ServerWorker>();
            Logger.LogTrace("Loaded: Service Worker");
        }

        Logger.LogInformation("Loaded hosted services");

        Logger.LogDebug("Loading services");

        foreach (var service in modules.GetServices<IService>())
        {
            Logger.LogTrace("Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogInformation("Loaded services");

        Logger.LogDebug("Loading event sinks");

        foreach (var eventSink in modules.GetServices<IEventSink>())
        {
            Logger.LogTrace("Loaded: {EventSink}", eventSink.Name);
            services.AddSingleton(eventSink);
        }

        Logger.LogInformation("Loaded event sinks");

        Logger.LogDebug("Loading modules");
        foreach (var service in modules.GetServices<Module>())
        {
            Logger.LogTrace("Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogInformation("Loaded modules");

        Logger.LogDebug("Loading Configs");

        foreach (var config in modules.GetServices<IRwConfig>())
            services.LoadConfigs(config, Logger);

        Logger.LogInformation("Loaded configs");

        Logger.LogDebug("Loading static configs");

        foreach (var staticConfig in modules.GetServices<IRConfig>())
            services.AddSingleton(staticConfig);

        Logger.LogInformation("Loaded configs");

        services
            .AddSingleton<Random>()
            .AddSingleton<TimerChangePool>()
            .AddSingleton<AccountAttackLimiter>()
            .AddSingleton<PasswordHasher>()
            .AddSingleton<FileLogger>()
            .AddSingleton<IpLimiter>();
    }

    public override void PostBuild(IServiceProvider services, Module[] modules)
    {
        foreach (var service in services.GetRequiredServices<IInjectModules>(modules))
            service.Modules = modules;

        foreach (var service in services.GetRequiredServices<IService>(modules))
            service.Initialize();

        services.GetRequiredService<ServerHandler>().SetModules(modules);
    }
}
