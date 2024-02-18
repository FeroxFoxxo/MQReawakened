using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
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

    public InternalLoot LootCatalog { get; set; }

    public override object[] GetInitData(Player player) => [Collected ? 0 : 1];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (Collected)
            return;

        Collected = true;

        player.GrantLoot(Id, LootCatalog, ItemCatalog, Logger);

        var triggerEvent = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true);

        //Temp way for adding bananas to empty chests to create a better user experience.
        if (string.IsNullOrEmpty(LootCatalog.GetLootById(Id).ObjectId))
        {
            var bananaReward = new Random().Next(30, 75);

            player.AddBananas(bananaReward);
            triggerEvent.EventDataList[0] = bananaReward;
        }
        Room.SendSyncEvent(triggerEvent);

        player.CheckObjective(ObjectiveEnum.InteractWith, Id, PrefabName, 1);

        var triggerReceiver = new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, player.GameObjectId.ToString(), true, 1f);
        Room.SendSyncEvent(triggerReceiver);
    }
}
