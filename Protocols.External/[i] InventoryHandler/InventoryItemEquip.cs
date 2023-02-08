using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Models.Planes;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._i__InventoryHandler;

internal class InventoryItemEquip : ExternalProtocol
{
    public override string ProtocolName => "ie";

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        if (message[5] == "0")
        {
            character.Data.Equipment.EquippedItems.Clear();
            character.Data.Equipment.EquippedBinding.Clear();
            return;
        }

        var equipDatas = message[5].Split(':');

        foreach(var equipData in equipDatas)
        {
            if (string.IsNullOrEmpty(equipData)) continue;

            HandleEquipData(equipData.Split("="), character);
        }
    }

    private void HandleEquipData(string[] equipData, CharacterModel character)
    {
        var subCategory = (ItemSubCategory)Convert.ToInt32(equipData[0]);
        var itemId = Convert.ToInt32(equipData[1]);
        var bindEquip = equipData[2] == "1";

        int equipedItemId;
        if (character.Data.Equipment.EquippedItems.ContainsKey(subCategory))
        {
            equipedItemId = character.Data.Equipment.EquippedItems[subCategory];
            character.Data.Equipment.EquippedItems[subCategory] = itemId;
        }
        else
        {
            equipedItemId = -1;
            character.Data.Equipment.EquippedItems.Add(subCategory, itemId);
        }

        if (bindEquip) character.Data.Equipment.EquippedBinding.Add(subCategory);

        if (equipedItemId > 0)
        {
            character.AddItem(ItemCatalog.GetItemFromId(equipedItemId), 1);
        }

        character.RemoveItem(itemId, 1);

        SendXt("iq", character.Data.Equipment);
        SendXt("ip", character.Data.Inventory.ToString().Replace('>', '|'), true);
    }
}
