using Achievement.Types;
using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using System.ComponentModel.Design;

namespace Server.Reawakened.Entities;

public class ChestControllerEntity : AbstractBaseChest<ChestController>
{
    public bool Collected;

    public Random Random { get; set; }

    public ItemCatalog ItemCatalog { get; set; }

    public ILogger<ChestControllerEntity> Logger { get; set; }

    public override object[] GetInitData(Player player) => new object[] { Collected ? 0 : 1 };

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (Collected)
            return;

        Collected = true;

        //var bananas = Random.Next(50, 101);
        //player.AddBananas(bananas);

        //string[] dummyItemsList = ["1009", "1870", "1872", "1874", "1876", "1878", "1830", "404", "1232"];

        var chestRewards = new Dictionary<string, object>
        {
            {"chestID", "default"},
            {"rewardType", "Item"},
            {"reward", new string[][] {
                ["1009", "1", "0"], ["1870", "1", "0"], ["1872", "1", "0"],
                ["1874", "1", "0"], ["1876", "1", "0"], ["1878", "1", "0"],
                ["1830", "1", "0"], ["404", "1", "0"], ["1232", "1", "0"] } 
            }
        };

        var rewardType = chestRewards["rewardType"];

        switch (rewardType)
        {
            case "Banana":
                {
                    var bananasReward = (string[])chestRewards["reward"];

                    var minBananas = int.Parse(bananasReward[0]);
                    var maxBananas = int.Parse(bananasReward[1]);
                    var bananas = Random.Next(minBananas, maxBananas);

                    player.AddBananas(bananas);

                    break;
                }

            case "Item":
                {
                    var items = (string[][])chestRewards["reward"];

                    string[][] gottenItems = [items[Random.Next(items.Length)]];
                    var itemsLooted = "";
                    var lootableItems = "";

                    foreach (var item in gottenItems)
                    {
                        var itemId = item[0];
                        var count = int.Parse(item[1]);
                        var bindingCount = item[2];

                        itemsLooted += $"{itemId}{{{count}{{{bindingCount}{{{DateTime.Now}|";
                        player.Character.AddItem(ItemCatalog.GetItemFromId(int.Parse(itemId)), count);
                    }

                    foreach (var item in items)
                    {
                        lootableItems += $"{item[0]}|";
                    }

                    player.SendXt("iW", itemsLooted, lootableItems, Id.ToString(), 0);
                    player.SendUpdatedInventory(false);

                    break;
                }
        }

        var trig = new Trigger_SyncEvent(Id.ToString(), Room.Time, true, player.GameObjectId.ToString(), true)
        { //need to redo trigger for ChestController
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
