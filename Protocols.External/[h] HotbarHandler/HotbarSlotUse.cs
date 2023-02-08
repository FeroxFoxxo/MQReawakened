using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

using A2m.Server;
using Server.Reawakened.Levels.Models.Planes;
using static LeaderBoardTopScoresJson;
using Server.Reawakened.Players.Models;
using System.Globalization;

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
        var position = new Vector3Model()
        {
            X = Convert.ToSingle(message[7], CultureInfo.InvariantCulture),
            Y = Convert.ToSingle(message[8], CultureInfo.InvariantCulture),
            Z = Convert.ToSingle(message[9], CultureInfo.InvariantCulture)
        };

        if (int.TryParse(message[5], out var hotbarSlotId))
        {
            //var targetUserId = Convert.ToInt32(message[6]); // Target user
            var slotItem = character.Data.Hotbar.HotbarButtons[hotbarSlotId];
            
            var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);

            switch(usedItem.SubCategoryId)
            {
            case ItemSubCategory.Potion:
            case ItemSubCategory.Elixir:
                HandleConsumablePotion(character, usedItem, hotbarSlotId);
                break;
            
            case ItemSubCategory.Offensive:
                HandleWeapon(usedItem, position);
                break;

            case ItemSubCategory.Defensive:
                HandleTrinket();
                break;

            default:
                File.AppendAllText("./use.txt", $"{usedItem.SubCategoryId}\n");
                break;
            }
        }
        else Logger.LogError("HotbarSlot ID must be an integer.");
    }

    private void HandleConsumablePotion(CharacterModel character, ItemDescription item, int hotbarSlotId)
    {
        character.Data.Inventory.Items[item.ItemId].Count--;

        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
        {
            character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);
            SendXt("hu", character.Data.Hotbar);

            character.Data.Inventory.Items[item.ItemId].Count = -1;
            SendXt("ip", character.Data.Inventory.ToString().Replace('>', '|'), false);

            character.Data.Inventory.Items.Remove(item.ItemId);
        }
        else SendXt("ip", character.Data.Inventory.ToString().Replace('>', '|'), false);
    }

    private void HandleWeapon(ItemDescription item, Vector3Model position)
    {
        var player = NetState.Get<Player>();
        
        var meleeTrigger = new Trigger_SyncEvent("13160481", player.CurrentLevel.Time, true, player.PlayerId.ToString(), true);
        
        var entts = player.CurrentLevel.LevelEntityHandler.GetEntities<TriggerCoopController>();

        NetState.SendSyncEventToPlayer(meleeTrigger);
        player.CurrentLevel.SendSyncEvent(meleeTrigger);
    }

    private void HandleTrinket()
    {

    }
}
