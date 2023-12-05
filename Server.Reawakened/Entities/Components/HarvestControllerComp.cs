using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components;
public class HarvestControllerComp : Component<HarvestController>
{
    public bool Collected;
    public ItemCatalog ItemCatalog { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ObjectiveCatalogInt ObjectiveCatalog { get; set; }
    public override object[] GetInitData(Player player) => new object[] { Collected ? 0 : 1 };
    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        base.RunSyncedEvent(syncEvent, player);

        var dailyCollectible = new Dailies_SyncEvent(syncEvent);
        Room.SendSyncEvent(dailyCollectible);

        switch (Entity.GameObject.ObjectInfo.PrefabName)
        {
            case "PF_GE_DailyOak01":
                player.Character.AddItem(ItemCatalog.GetItemFromId(510), 1);
                break;
            case "PF_GE_DailyApple01":
                player.Character.AddItem(ItemCatalog.GetItemFromId(1568), 3);
                break;
            case "PF_GE_DailyCopperOre01":
                player.Character.AddItem(ItemCatalog.GetItemFromId(110), 1);
                break;
        }
        player.SendUpdatedInventory(false);

        player.CheckObjective(QuestCatalog, ObjectiveCatalog, A2m.Server.ObjectiveEnum.Collect, Id, PrefabName, 1);
    }
}
