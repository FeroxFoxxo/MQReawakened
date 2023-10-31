using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities;

public class ChestControllerEntity : AbstractBaseChest<ChestController>
{
    public bool Collected;

    public Random Random { get; set; }

    public ItemCatalog ItemCatalog { get; set; }

    public override object[] GetInitData(Player player) => new object[] { Collected ? 0 : 1 };

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (Collected)
            return;

        Collected = true;

        var bananas = Random.Next(50, 101);
        player.AddBananas(bananas);

        var rdmItem = Random.Next(1, 7);
        switch (rdmItem)
        {
            case 1:
                player.Character.AddItem(ItemCatalog.GetItemFromId(1568), Random.Next(1, 4));
                break;
            case 2:
                player.Character.AddItem(ItemCatalog.GetItemFromId(1803), 1);
                break;
            case 3:
                player.Character.AddItem(ItemCatalog.GetItemFromId(404), 1);
                break;
            case 4:
                player.Character.AddItem(ItemCatalog.GetItemFromId(1070), 1);
                break;
            case 5:
                player.Character.AddItem(ItemCatalog.GetItemFromId(2871), 1);
                break;
            case 6:
                player.Character.AddItem(ItemCatalog.GetItemFromId(1576), 1);
                break;
        }
        player.SendUpdatedInventory(false);

        var trig = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true)
        {
            EventDataList =
            {
                [0] = bananas
            }
        };

        Room.SendSyncEvent(trig);

        var rec = new TriggerReceiver_SyncEvent(Id.ToString(), Room.Time, player.GameObjectId.ToString(), true, 1f);

        Room.SendSyncEvent(rec);
    }
}
