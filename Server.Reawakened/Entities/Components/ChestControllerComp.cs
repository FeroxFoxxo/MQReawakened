using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components;

public class ChestControllerComp : BaseChestControllerComp<ChestController>
{
    public bool Collected;

    public ItemCatalog ItemCatalog { get; set; }

    public ILogger<ChestControllerComp> Logger { get; set; }

    public LootCatalogInt LootCatalog { get; set; }

    public override object[] GetInitData(Player player) => [Collected ? 0 : 1];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (Collected)
            return;

        Collected = true;

        player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);

        var trig = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true)
        {
            //need to redo trigger for ChestController
            /*
            EventDataList =
            {
                [0] = bananas
            }
            */
        };

        Room.SendSyncEvent(trig);

        var rec = new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, player.GameObjectId.ToString(), true, 1f);

        Room.SendSyncEvent(rec);
    }
}
