using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.GameObjects.Items.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

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
            var bananaReward = RandomBananaReward(player);

            player.AddBananas(bananaReward, InternalAchievement, Logger);
            triggerEvent.EventDataList[0] = bananaReward;
        }

        if (PrefabName.Contains(ServerRConfig.DailyBoxName) || IsLoyaltyChest == true)
            player.Character.CurrentCollectedDailies.TryAdd(Id, SetDailyHarvest(Id, Room.LevelInfo.LevelId, DateTime.Now));

        Room.SendSyncEvent(triggerEvent);
        Room.SendSyncEvent(triggerReceiver);

        player.SendUpdatedInventory();
    }

    //Temporary for chests without loot tables. Could be used for banana rewards in the future.
    public int RandomBananaReward(Player player)
    {
        var random = new Random();
        var bananaReward = PrefabName switch
        {
            var prefab when prefab.Contains(ServerRConfig.BlueChestName) => random.Next(85, 155),
            var prefab when prefab.Contains(ServerRConfig.PurpleChestName) => random.Next(295, 500),
            _ => random.Next(45, 70),
        };

        if (player.Character.GlobalLevel > ServerRConfig.DoubleChestRewardsLevel)
            bananaReward *= 2;

        return bananaReward;
    }
}
