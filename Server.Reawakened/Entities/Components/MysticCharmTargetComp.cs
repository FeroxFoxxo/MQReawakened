using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Components;

public class MysticCharmTargetComp : Component<MysticCharmTarget>
{
    public Vector3 CollisionSize => ComponentData.CollisionSize;
    public Vector3 CollisionCenter => ComponentData.CollisionCenter;
    public float CollisionRemovalDelay => ComponentData.CollisionRemovalDelay;

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var trigger = new Trigger_SyncEvent(syncEvent);

        if (trigger.Activate)
            Charm(player);
    }

    public void Charm(Player player)
    {
        var syncEvent = new SyncEvent(Id.ToString(), SyncEvent.EventType.Charm, Room.Time);
        syncEvent.EventDataList.Add(1);
        syncEvent.EventDataList.Add(398);

        var charmEvent = new Charm_SyncEvent(syncEvent);
        player.SendSyncEventToPlayer(charmEvent);
    }
}
