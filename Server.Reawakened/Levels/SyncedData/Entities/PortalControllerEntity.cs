using Server.Base.Network;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.SyncedData.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Levels.SyncedData.Entities;

public class PortalControllerEntity : SynchronizedEntity<PortalController>
{
    public PortalControllerEntity(StoredEntityModel storedEntity,
        PortalController entityData) : base(storedEntity, entityData)
    {
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        var portal = new Portal_SyncEvent(syncEvent);

        var levelInfo = Level.WorldGraph.GetInfoLevel(portal.LevelName);
        var level = Level.LevelHandler.GetLevelFromId(levelInfo.LevelId);

        player.JoinLevel(netState, level, out var reason);

        if (reason == JoinReason.Accepted)
            netState.SendLevelChange(level.LevelHandler, level.WorldGraph);
    }
}
