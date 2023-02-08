using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

using A2m.Server;

namespace Protocols.External._h__HotbarHandler;

internal class HotbarSlotUse : ExternalProtocol
{
    public override string ProtocolName => "hu";

    public ILogger<HotbarSlotUse> Logger { get; set; }

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var character = player.GetCurrentCharacter();

        // position where to spawn FX
        //var posX = Convert.ToInt32(message[7]);
        //var posY = Convert.ToInt32(message[8]);
        //var posZ = Convert.ToInt32(message[9]);

        if (int.TryParse(message[5], out var hotbarSlotId))
        {
            //var targetUserId = Convert.ToInt32(message[6]); // Target user
            var slotItem = character.Data.Hotbar.HotbarButtons[hotbarSlotId];
            
            var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);

            if (usedItem.SubCategoryId is ItemSubCategory.Potion or ItemSubCategory.Elixir)
            {
                character.Data.Inventory.Items[slotItem.ItemId].Count--;

                if (character.Data.Inventory.Items[slotItem.ItemId].Count <= 0)
                {
                    character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);
                    SendXt("hu", character.Data.Hotbar);

                    character.Data.Inventory.Items[slotItem.ItemId].Count = -1;
                    SendXt("ip", character.Data.Inventory.ToString().Replace('>', '|'), false);

                    character.Data.Inventory.Items.Remove(slotItem.ItemId);
                }
                else SendXt("ip", character.Data.Inventory.ToString().Replace('>', '|'), false);
            }
        }
        else Logger.LogError("HotbarSlot ID must be an integer.");
    }
}
