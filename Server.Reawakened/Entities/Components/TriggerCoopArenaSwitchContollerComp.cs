using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;
public class TriggerCoopArenaSwitchControllerComp : Component<TriggerCoopArenaSwitchController>
{
    public string ArenaObjectId => ComponentData.ArenaObjectID;
    public bool Enabled => ComponentData.Enabled;

    public override object[] GetInitData(Player player) => base.GetInitData(player);

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        base.RunSyncedEvent(syncEvent, player);

        var triggerSyncEvent = new Trigger_SyncEvent(syncEvent);
        player.Room.SendSyncEvent(triggerSyncEvent);

        var arenaActivation = Convert.ToInt32(syncEvent.EventDataList[2]);

        if (arenaActivation > 0)
            HandleMiniGameRace(player);
    }

    public async void HandleMiniGameRace(Player player)
    {
        var startRace = new Trigger_SyncEvent(ArenaObjectId.ToString(), player.Room.Time,
            true, player.GameObjectId.ToString(), player.Room.LevelInfo.LevelId, true, true);

        player.Room.SendSyncEvent(startRace);
    }
}
