using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Network.Extensions;
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

    public ILogger<ChestControllerEntity> Logger { get; set; }

    public override object[] GetInitData(Player player) => new object[] { Collected ? 0 : 1 };

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (Collected)
            return;

        Collected = true;

        //Temporary Loot Wheel System.
        string[] LootableItems1 = { "3009{1{0{0|", "1870{1{0{0|", "3082{1{0{0|",   //8 items.
            "1621{1{0{0|", "597{1{0{0|", "1830{1{0{0|", "404{1{0{0|", "1232{1{0{0|"};

        string[] LootableItems2 = { "3133{1{0{0|", "1164{1{0{0|", "2928{1{0{0|",   //6 items.
            "750{1{0{0|", "3048{1{0{0|", "1829{1{0{0|"};

        string[] LootableItems3 = { "220{1{0{0|", "405{1{0{0|", "442{1{0{0|",      //7 items.
            "474{1{0{0|", "478{1{0{0|", "583{1{0{0|", "1828{1{0{0|" };

        string[] LootableItems4 = { "586{1{0{0|", "588{1{0{0|", "590{1{0{0|",      //8 items.
            "1734{1{0{0|", "1797{1{0{0|", "613{1{0{0|", "612{1{0{0|", "611{1{0{0|" };

        var wheelOfLoot1 = "3009|1870|1872|1874|1876|1830|404|1232";
        var wheelOfLoot2 = "3133|1164|2928|750|3048|1621|404";
        var wheelOfLoot3 = "220|405|442|474|478|583|597";
        var wheelOfLoot4 = "586|588|590|1734|1797|613|612|611";

        var randomWheelOfLoot = "";

        var randomLootWheel = Random.Next(0, 4);     
        switch (randomLootWheel)
        {
            case 0:
                randomWheelOfLoot = wheelOfLoot1;
                break;
            case 1:
                randomWheelOfLoot = wheelOfLoot2;
                break;
            case 2:
                randomWheelOfLoot = wheelOfLoot3;
                break;
            case 3:
                randomWheelOfLoot = wheelOfLoot4;
                break;
            default:
                Logger.LogDebug("No loot wheel for randomLootWheel: {args1}", randomLootWheel);
                break;
        }
        var randomItem1 = Random.Next(0, 8);
        var randomItem2 = Random.Next(0, 6);
        var randomItem3 = Random.Next(0, 7);
        string[] tempItemOfLootArray = { LootableItems1[randomItem1], LootableItems2[randomItem2],
                LootableItems3[randomItem3], LootableItems4[randomItem1] }; //item from wheel

        var randomItemFromWheel = tempItemOfLootArray[randomLootWheel];

        Logger.LogDebug("WheelOfLoot: {arg1} Item: {arg2}", randomWheelOfLoot, randomItemFromWheel);

        player.SendXt("iW", randomItemFromWheel, randomWheelOfLoot, Id.ToString(), 0);

        var bananas = Random.Next(50, 121);
        player.AddBananas(bananas);

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
