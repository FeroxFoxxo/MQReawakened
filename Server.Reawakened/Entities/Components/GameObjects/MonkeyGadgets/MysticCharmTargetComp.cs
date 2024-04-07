using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.MonkeyGadgets;

public class MysticCharmTargetComp : Component<MysticCharmTarget>
{
    public Vector3 CollisionSize => ComponentData.CollisionSize;
    public Vector3 CollisionCenter => ComponentData.CollisionCenter;
    public float CollisionRemovalDelay => ComponentData.CollisionRemovalDelay;

    public ItemCatalog ItemCatalog { get; set; }

    private int _charmItemId;

    public override void InitializeComponent() =>
        _charmItemId = ItemCatalog.GetItemFromPrefabName("ABIL_MysticCharm01").ItemId;

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
        syncEvent.EventDataList.Add(_charmItemId);

        var charmEvent = new Charm_SyncEvent(syncEvent);
        player.SendSyncEventToPlayer(charmEvent);
    }
}
