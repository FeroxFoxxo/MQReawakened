using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Web.Abstractions;
using Web.Razor.Services;
using Microsoft.AspNetCore.Builder;
using Web.Razor.Hubs;
using Server.Colliders.Abstractions;

namespace Web.Razor;

public class Razor(ILogger<Razor> logger) : WebModule(logger)
{
    public override void AddServices(IServiceCollection services, Module[] modules)
    {
        services.AddSingleton<PagesService>()
            .AddSingleton<IColliderUpdatePublisher, SignalRColliderPublisher>();

        services.AddSignalR();
        services.AddRazorPages();
    }

    public override void PostWebBuild(WebApplication app)
    {
        base.PostWebBuild(app);
        app.MapRazorPages();
        app.MapHub<ColliderHub>("/hubs/colliders");
    }
}
