using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using System;

namespace Server.Reawakened.Entities.Components
{
    public class HarvestControllerComp : BaseChestControllerComp<HarvestController>
    {
        public ItemCatalog ItemCatalog { get; set; }
        public InternalLoot LootCatalog { get; set; }
        public ILogger<HarvestControllerComp> Logger { get; set; }

        public override object[] GetInitData(Player player) => [ActivateDailyHarvest(player) ? 1 : 0];

        public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
        {
            base.RunSyncedEvent(syncEvent, player);
            Room.SendSyncEvent(new Dailies_SyncEvent(syncEvent));

            player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);
            player.SendUpdatedInventory(false);
            player.CheckObjective(A2m.Server.ObjectiveEnum.Collect, Id, PrefabName, 1);

            player.Character.Data.CurrentCollectedDailies.TryAdd(Id, DateTime.Now);
        }

        public bool ActivateDailyHarvest(Player player)
        {
            if (player.Character.Data.CurrentCollectedDailies == null)
                player.Character.Data.CurrentCollectedDailies = new Dictionary<string, DateTime>();

            if (!player.Character.Data.CurrentCollectedDailies.TryGetValue(Id, out var time))
                return true;

            var nextHarvestedTime = time + TimeSpan.FromDays(1);

            if (player.Character.Data.CurrentCollectedDailies[Id] >= nextHarvestedTime)
                return true;

            Logger.LogInformation("Daily of Id ({Id}) isn't ready for harvest. Return on ({nextHarvestedTime}) to harvest.", Id, nextHarvestedTime);

            return false;
        }
    }
}
