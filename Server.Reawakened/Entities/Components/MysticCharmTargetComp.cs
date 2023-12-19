using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Components;

public class MysticCharmTargetComp : Component<MysticCharmTarget>
{
    public Vector3 CollisionSize => ComponentData.CollisionSize;
    public Vector3 CollisionCenter => ComponentData.CollisionCenter;
    public float CollisionRemovalDelay => ComponentData.CollisionRemovalDelay;

    public bool IsOpened = false;
    private int _timer;
    public override void InitializeComponent()
    {
        base.InitializeComponent();
        Uncharm();
    }
    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        Charm(10);
        base.RunSyncedEvent(syncEvent, player);
    }
    public void Charm(int time)
    {
        IsOpened = true;
        var syncEvent = new SyncEvent(Id.ToString(), SyncEvent.EventType.Charm, Room.Time);
        syncEvent.EventDataList.Add(1);
        syncEvent.EventDataList.Add(398);
        var charmEvent = new Charm_SyncEvent(syncEvent);
        Room.SendSyncEvent(charmEvent);
        _timer = (int)(time + CollisionRemovalDelay)*60;
    }
    public void Uncharm()
    {
        IsOpened = false;
        var syncEvent = new SyncEvent(Id.ToString(), SyncEvent.EventType.Charm, Room.Time);
        syncEvent.EventDataList.Add(0);
        syncEvent.EventDataList.Add(398);
        var charmEvent = new Charm_SyncEvent(syncEvent);
        Room.SendSyncEvent(charmEvent);
    }
}
