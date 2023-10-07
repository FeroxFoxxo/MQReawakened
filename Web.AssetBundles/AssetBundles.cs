using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Web.Abstractions;
using System.Runtime.CompilerServices;
using Web.AssetBundles.Events;

namespace Web.AssetBundles;

public class AssetBundles(ILogger<AssetBundles> logger) : WebModule(logger)
{
    public override string[] Contributors { get; } = ["Ferox", "Prefare"];

    public override void AddServices(IServiceCollection services, Module[] modules)
    {
        services.AddSingleton<AssetEventSink>();

        Logger.LogDebug("Loading bundles");

        foreach (var xml in modules.GetServices<IBundledXml>())
        {
            Logger.LogTrace("   Loaded: {ServiceName}", xml.Name);
            services.AddSingleton(xml, RuntimeHelpers.GetUninitializedObject(xml));
        }

        Logger.LogInformation("Loaded bundles");
    }
}
