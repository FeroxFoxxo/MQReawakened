using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using static CollectibleController;

namespace Server.Reawakened.Entities.Components;
public class QuestCollectibleControllerComp : Component<QuestCollectibleController>
{
    public bool Collected = false;
    public string CollectedFx => ComponentData.CollectedFx;
    public float GatherTime => ComponentData.GatherTime;

    public override object[] GetInitData(Player player) => [(int)(Collected ? CollectibleState.Collected : CollectibleState.Active)];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        player.CheckObjective(ObjectiveEnum.Receiveitem, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.Collect, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.AlterandReceiveitem, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.InteractWith, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.Deliver, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.Alter, Id, PrefabName, 1);

        var questCollectible = new Trigger_SyncEvent(syncEvent);
        Room.SendSyncEvent(questCollectible);

        Collected = true;
    }
}
