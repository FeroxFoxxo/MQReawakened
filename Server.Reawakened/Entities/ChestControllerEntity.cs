using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.LootHandlers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities;

public class ChestControllerEntity : AbstractBaseChest<ChestController>
{
    public bool Collected;

    public ItemCatalog ItemCatalog { get; set; }

    public ILogger<ChestControllerEntity> Logger { get; set; }

    public InternalLootCatalog LootCatalog { get; set; }

    public override object[] GetInitData(Player player) => new object[] { Collected ? 0 : 1 };

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
