using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components;
public class HarvestControllerComp : BaseChestControllerComp<HarvestController>
{
    public bool Collected;
    public ItemCatalog ItemCatalog { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ObjectiveCatalogInt ObjectiveCatalog { get; set; }
    public LootCatalogInt LootCatalog { get; set; }
    public ILogger<HarvestControllerComp> Logger { get; set; }
    public override object[] GetInitData(Player player) => [Collected ? 0 : 1];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        base.RunSyncedEvent(syncEvent, player);

        var dailyCollectible = new Dailies_SyncEvent(syncEvent);
        Room.SendSyncEvent(dailyCollectible);

        player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);

        player.SendUpdatedInventory(false);

        player.CheckObjective(QuestCatalog, ObjectiveCatalog, A2m.Server.ObjectiveEnum.Collect, Id, PrefabName, 1);
    }
}
