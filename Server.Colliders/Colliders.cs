using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Colliders.Services;

namespace Server.Colliders;

public class Colliders(ILogger<Colliders> logger) : Module(logger)
{
    public override void AddServices(IServiceCollection services, Module[] modules) =>
        services.AddSingleton<ColliderSnapshotProvider>()
            .AddSingleton<InMemoryRoomVersionTracker>()
            .AddSingleton<InMemoryColliderSubscriptionTracker>()
            .AddHostedService<ColliderPushService>();
}
