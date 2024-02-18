using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class MiniQuestControllerComp : Component<MiniQuestController>
{
    public int MaxHit => ComponentData.MaxHit;
    public float TimeDelay => ComponentData.TimeDelay;
    public string Endpoint => ComponentData.Endpoint;

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (syncEvent.Type != SyncEvent.EventType.TriggerReceiver)
            return;

        var recievedEvent = new TriggerReceiver_SyncEvent(syncEvent);

        player.SendSyncEventToPlayer(syncEvent);

        if (recievedEvent.Activate)
            player.CheckObjective(ObjectiveEnum.Score, Id, PrefabName, 1);
    }
}
