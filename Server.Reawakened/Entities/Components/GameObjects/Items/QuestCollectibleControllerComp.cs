using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using static CollectibleController;

namespace Server.Reawakened.Entities.Components.GameObjects.Items;
public class QuestCollectibleControllerComp : Component<QuestCollectibleController>
{
    public CollectibleState CollectedState;
    public string CollectedFx => ComponentData.CollectedFx;
    public float GatherTime => ComponentData.GatherTime;

    public QuestCatalog QuestCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    private ItemDescription _questItem;

    public override object[] GetInitData(Player player)
    {
        CollectedState = CollectibleState.NotActive;

        _questItem = ItemCatalog.GetItemFromPrefabName(PrefabName);

        foreach (var objective in player.Character.QuestLog.SelectMany(x => x.Objectives.Values).Where
            (x => x.GameObjectId.ToString() == Id || _questItem != null && x.ItemId == _questItem.ItemId))
            CollectedState = CollectibleState.Active;

        return [UpdateActiveObjectives(player, CollectedState)];
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var count = 1;

        if (_questItem != null)
        {
            player.AddItem(_questItem, count, ItemCatalog);
            player.SendUpdatedInventory();
        }

        player.CheckObjective(ObjectiveEnum.Collect, Id, PrefabName, count, QuestCatalog);
        player.CheckObjective(ObjectiveEnum.InteractWith, Id, PrefabName, count, QuestCatalog);

        player.CheckObjective(ObjectiveEnum.Deliver, Id, PrefabName, count, QuestCatalog);

        player.CheckObjective(ObjectiveEnum.Alter, Id, PrefabName, count, QuestCatalog);
        player.CheckObjective(ObjectiveEnum.AlterandReceiveitem, Id, PrefabName, count, QuestCatalog);

        player.CheckObjective(ObjectiveEnum.Receiveitem, Id, PrefabName, count, QuestCatalog);
        player.CheckObjective(ObjectiveEnum.Giveitem, Id, PrefabName, count, QuestCatalog);

        player.SendSyncEventToPlayer(new Trigger_SyncEvent(syncEvent));

        CollectedState = CollectibleState.Collected;

        UpdateActiveObjectives(player, CollectedState);
    }

    public int UpdateActiveObjectives(Player player, CollectibleState collectedState)
    {
        var questItem = ItemCatalog.GetItemFromPrefabName(PrefabName);

        foreach (var objective in player.Character.QuestLog.SelectMany(x => x.Objectives.Values).Where
            (x => x.GameObjectId.ToString() == Id || questItem != null && x.ItemId == questItem.ItemId))
            switch (collectedState)
            {
                case CollectibleState.NotActive:
                    player.SendSyncEventToPlayer(new Trigger_SyncEvent(Id.ToString(), player.Room.Time,
                        false, player.GameObjectId.ToString(), false));
                    break;

                case CollectibleState.Active:
                    player.SendSyncEventToPlayer(new Trigger_SyncEvent(Id.ToString(), player.Room.Time,
                        true, player.GameObjectId.ToString(), true));
                    break;

                case CollectibleState.Collected:
                    player.SendSyncEventToPlayer(new Trigger_SyncEvent(Id.ToString(), player.Room.Time,
                        true, player.GameObjectId.ToString(), false));
                    break;
            }

        return (int)collectedState;
    }
}
