using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Colliders.Abstractions;
using Server.Colliders.Services;

namespace Server.Colliders;

public class Colliders(ILogger<Colliders> logger) : Module(logger)
{
    public override void AddServices(IServiceCollection services, Module[] modules)
    {
        services.AddSingleton<IColliderSnapshotProvider, ColliderSnapshotProvider>();
        services.AddSingleton<IColliderDiffCalculator, ColliderDiffCalculator>();
        services.AddSingleton<IRoomVersionTracker, InMemoryRoomVersionTracker>();
        services.AddHostedService<ColliderPushService>();
    }
}
