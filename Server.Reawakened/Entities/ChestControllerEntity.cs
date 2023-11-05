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
using System.Collections;

namespace Server.Reawakened.Entities;

public class ChestControllerEntity : AbstractBaseChest<ChestController>
{
    public bool Collected;

    public Random Random { get; set; }

    public ItemCatalog ItemCatalog { get; set; }

    public ILogger<ChestControllerEntity> Logger { get; set; }

    public InternalLootCatalog LootCatalog { get; set; }

    public override object[] GetInitData(Player player) => new object[] { Collected ? 0 : 1 };

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (Collected)
            return;

        Collected = true;

        var chestRewards = LootCatalog.GetLootById(Id);

        var rewardType = chestRewards["rewardType"];

        switch (rewardType)
        {
            case "Banana": //Banana reward not properly functioning yet
                {
                    object[] reward = chestRewards["reward"];
                    string[] bananasReward = Array.ConvertAll(reward, x => x.ToString());
                    
                    var minBananas = Convert.ToInt32(bananasReward[0]);
                    var maxBananas = Convert.ToInt32(bananasReward[1]);
                    var bananas = Random.Next(minBananas, maxBananas);

                    player.AddBananas(bananas);

                    break;
                }

            case "Item":
                {
                    object[] reward = chestRewards["reward"];

                    int[][] items = Array.ConvertAll(reward, childArr =>
                        {
                            return Array.ConvertAll<int, int> ( (int[])childArr, number => Convert.ToInt32(number));
                        });

                    var gottenItems = new int[][] { items[Random.Next(items.Length)] };
                    var itemsLooted = "";
                    var lootableItems = "";

                    foreach (var item in gottenItems)
                    {
                        var itemId = item[0];
                        var count = item[1];
                        var bindingCount = item[2];

                        itemsLooted += $"{itemId}{{{count}{{{bindingCount}{{{DateTime.Now}|";
                        player.Character.AddItem(ItemCatalog.GetItemFromId(itemId), count);
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
