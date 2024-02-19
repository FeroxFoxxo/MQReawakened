using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using static BaseChestController;

namespace Server.Reawakened.Entities.Components;

public class ChestControllerComp : BaseChestControllerComp<ChestController>
{
    public int ChestState;
    public ItemCatalog ItemCatalog { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ILogger<ChestControllerComp> Logger { get; set; }

    public override object[] GetInitData(Player player)
    {
        ChestState = (int)DailiesState.Active;

        if (player.Character.CurrentCollectedDailies != null)
            foreach (var dailyHarvest in player.Character.CurrentCollectedDailies)
            {
                if (dailyHarvest.Key.GameObjectId == Id && dailyHarvest.Key.LevelId == Room.LevelInfo.LevelId)
                    Console.WriteLine("True Test");

                else
                    Console.WriteLine("False Test");
            }

        if (PrefabName.Contains(ServerRConfig.DailyBoxPrefabName) && player.Character.CurrentCollectedDailies.ContainsKey
            (player.Character.GetDailyHarvest(Id, Room.LevelInfo.LevelId)))
        {
            ChestState = (int)DailiesState.Collected;

            if (player.Character.CanActivateDailies(player, player.Character.GetDailyHarvest(Id, Room.LevelInfo.LevelId)))
                ChestState = (int)DailiesState.Active;
        }

        return [ChestState];
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);

        player.CheckObjective(ObjectiveEnum.InteractWith, Id, PrefabName, 1);

        var triggerEvent = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true);

        //Temp way for adding bananas to empty chests to create a better user experience.
        if (string.IsNullOrEmpty(LootCatalog.GetLootById(Id).ObjectId))
        {
            var bananaReward = new Random().Next(30, 55);

            player.AddBananas(bananaReward);
            triggerEvent.EventDataList[0] = bananaReward;
        }

        var triggerReceiver = new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, player.GameObjectId.ToString(), true, 1f);

        if (PrefabName.Contains(ServerRConfig.DailyBoxPrefabName))
        {
            player.SendSyncEventToPlayer(triggerEvent);
            player.SendSyncEventToPlayer(triggerReceiver);

            if (!player.Character.CurrentCollectedDailies.ContainsKey(player.Character.GetDailyHarvest(Id, Room.LevelInfo.LevelId)))
                player.Character.CurrentCollectedDailies.Add(player.Character.GetDailyHarvest(Id, Room.LevelInfo.LevelId), DateTime.Now);

            return;
        }

        Room.SendSyncEvent(triggerEvent);
        Room.SendSyncEvent(triggerReceiver);
    }
}
