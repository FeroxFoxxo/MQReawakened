using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Thrift.Abstractions;
using SmartFoxClientAPI;

namespace Server.Reawakened;

public class Reawakened(ILogger<Reawakened> logger) : Module(logger)
{
    public override string[] Contributors { get; } = ["Ferox"];

    public override void AddServices(IServiceCollection services, Module[] modules)
    {
        Logger.LogDebug("Loading thrift handlers");

        foreach (var service in modules.GetServices<ThriftHandler>())
        {
            Logger.LogTrace("   Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogInformation("Loaded thrift handlers");

        services
            .AddSingleton<ReflectionUtils>()
            .AddSingleton<SmartFoxClient>()
            .AddSingleton<DatabaseContainer>()
            .AddSingleton<NameGenSyllables>();
    }

    public override void PostBuild(IServiceProvider services, Module[] modules)
    {
        foreach (var thrift in services.GetRequiredServices<ThriftHandler>(modules))
        {
            thrift.AddProcesses(thrift.ProcessMap);

            Logger.LogTrace("   Added {Count} thrift protocols to {Handler}",
                thrift.ProcessMap.Count, thrift.GetType().Name);
        }
    }

    public override string GetModuleInformation() =>
        $"{base.GetModuleInformation()} for API {new SmartFoxClient().GetVersion()}";
}
