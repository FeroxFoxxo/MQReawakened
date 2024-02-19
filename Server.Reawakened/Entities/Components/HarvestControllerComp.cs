using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components
{
    public class HarvestControllerComp : BaseChestControllerComp<HarvestController>
    {
        public ItemCatalog ItemCatalog { get; set; }
        public InternalLoot LootCatalog { get; set; }
        public ILogger<HarvestControllerComp> Logger { get; set; }

        public override object[] GetInitData(Player player) => [player.Character.CanActivateDailies
            (player, player.Character.GetDailyHarvest(Id, Room.LevelInfo.LevelId)) ? (int)DailiesState.Active : (int)DailiesState.Collected];


        public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
        {
            base.RunSyncedEvent(syncEvent, player);
            player.SendSyncEventToPlayer(new Dailies_SyncEvent(syncEvent));

            player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
            player.SendUpdatedInventory(false);
            player.CheckObjective(A2m.Server.ObjectiveEnum.Collect, Id, PrefabName, 1);

            if (!player.Character.CurrentCollectedDailies.ContainsKey(player.Character.GetDailyHarvest(Id, Room.LevelInfo.LevelId)))
                player.Character.CurrentCollectedDailies.Add(player.Character.GetDailyHarvest(Id, Room.LevelInfo.LevelId), DateTime.Now);
        }
    }
}

