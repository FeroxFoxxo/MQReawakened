using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.GameObjects.Items.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Server.Reawakened.Entities.Components.GameObjects.Items;

public class HarvestControllerComp : BaseChestControllerComp<HarvestController>
{
    public ItemCatalog ItemCatalog { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<HarvestControllerComp> Logger { get; set; }

    public override object[] GetInitData(Player player) => [CanActivateDailies(player, Id)
        ? (int)DailiesState.Active : (int)DailiesState.Collected];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        base.RunSyncedEvent(syncEvent, player);
        player.SendSyncEventToPlayer(new Dailies_SyncEvent(syncEvent));

        player.GrantLoot(Id, LootCatalog, ItemCatalog, InternalAchievement, Logger);
        player.SendUpdatedInventory();
        player.CheckObjective(ObjectiveEnum.Collect, Id, PrefabName, 1, QuestCatalog);

        player.Character.CurrentCollectedDailies.TryAdd(Id, SetDailyHarvest(Id, Room.LevelInfo.LevelId, DateTime.Now));
    }
}
