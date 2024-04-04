using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components.GameObjects.Items;

public class ChestControllerComp : BaseChestControllerComp<ChestController>
{
    public DailiesState ChestState;
    public ItemCatalog ItemCatalog { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<ChestControllerComp> Logger { get; set; }

    public override object[] GetInitData(Player player)
    {
        ChestState = DailiesState.Active;

        if (player.Character.CurrentCollectedDailies != null &&
            PrefabName.Contains(ServerRConfig.DailyBoxName) || IsLoyaltyChest == true)
            if (!CanActivateDailies(player, Id))
                ChestState = DailiesState.Collected;

        return [(int)ChestState];
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        player.GrantLoot(Id, LootCatalog, ItemCatalog, InternalAchievement, Logger);

        player.CheckObjective(ObjectiveEnum.InteractWith, Id, PrefabName, 1, QuestCatalog);

        var triggerEvent = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true);
        var triggerReceiver = new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, player.GameObjectId.ToString(), true, 1f);

        //Temp way for adding bananas to empty chests to create a better user experience.
        if (string.IsNullOrEmpty(LootCatalog.GetLootById(player.Room.LevelInfo.LevelId, Id).ObjectId))
        {
            var bananaReward = new Random().Next(30, 55);

            player.AddBananas(bananaReward, InternalAchievement, Logger);
            triggerEvent.EventDataList[0] = bananaReward;
        }

        if (PrefabName.Contains(ServerRConfig.DailyBoxName) || IsLoyaltyChest == true)
        {
            player.SendSyncEventToPlayer(triggerEvent);
            player.SendSyncEventToPlayer(triggerReceiver);

            player.Character.CurrentCollectedDailies.TryAdd(Id, SetDailyHarvest(Id, Room.LevelInfo.LevelId, DateTime.Now));

            return;
        }

        Room.SendSyncEvent(triggerEvent);
        Room.SendSyncEvent(triggerReceiver);
    }
}
