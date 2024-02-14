using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using static CollectibleController;

namespace Server.Reawakened.Entities.Components;
public class QuestCollectibleControllerComp : Component<QuestCollectibleController>
{
    public int CollectedState;
    public bool Collected;
    public string CollectedFx => ComponentData.CollectedFx;
    public float GatherTime => ComponentData.GatherTime;

    public override object[] GetInitData(Player player) => [UpdateActiveObjectives(player, false)];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        player.CheckObjective(ObjectiveEnum.Receiveitem, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.Collect, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.AlterandReceiveitem, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.InteractWith, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.Deliver, Id, PrefabName, 1);
        player.CheckObjective(ObjectiveEnum.Alter, Id, PrefabName, 1);

        var questCollectible = new Trigger_SyncEvent(syncEvent);
        player.SendSyncEventToPlayer(questCollectible);

        Collected = true;

        UpdateActiveObjectives(player, true);
    }

    public int UpdateActiveObjectives(Player player, bool collect)
    {
        foreach (var objective in player.Character.Data.QuestLog.SelectMany(x => x.Objectives.Values))
        {
            var item = player.DatabaseContainer.ItemCatalog.GetItemFromPrefabName(PrefabName);

            if (objective.GameObjectId.ToString() == Id &&
                objective.GameObjectLevelId == player.Room.LevelInfo.LevelId ||
                item != null && item.ItemId == objective.ItemId)
            {
                if (!collect)
                {
                    if (!player.TempData.ActiveObjectives.ContainsKey(Id))
                        player.TempData.ActiveObjectives.Add(Id, true);

                    CollectedState = (int)CollectibleState.Active;

                    player.SendSyncEventToPlayer(new Trigger_SyncEvent(Id.ToString(), player.Room.Time,
                        true, player.GameObjectId.ToString(), true));
                }

                else if (collect)
                {
                    if (player.TempData.ActiveObjectives.ContainsKey(Id))
                        player.TempData.ActiveObjectives.Remove(Id);

                    CollectedState = (int)CollectibleState.Collected;

                    player.SendSyncEventToPlayer(new Trigger_SyncEvent(Id.ToString(), player.Room.Time,
                        false, player.GameObjectId.ToString(), false));
                }
            }
        }

        return CollectedState;
    }
}
