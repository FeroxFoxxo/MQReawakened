using A2m.Server;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class Hotbar : SlashCommand
{
    public override string CommandName => "/Hotbar";

    public override string CommandDescription => "Unlocks all slots and will set an item in a hotbar slot.";

    public override List<ParameterModel> Parameters => [
        new ParameterModel() {
            Name = "hotbarNum",
            Description = "The hotbar slot id."
        },
        new ParameterModel() {
            Name = "itemId",
            Description = "The item id."
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ItemRConfig ItemRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        player.AddSlots(true, ItemRConfig);

        if (args.Length <= 2)
            return;

        if (!int.TryParse(args[1], out var hotbarId) || !int.TryParse(args[2], out var itemId) || hotbarId is < 1 or > 5)
        {
            Log("Please enter a hotbar number from 1-5 and an item Id.", player);
            return;
        }

        var item = ItemCatalog.GetItemFromId(itemId);

        if (item == null)
        {
            Log($"No item with id '{itemId}' could be found.", player);
            return;
        }

        if (hotbarId == 5 && item.InventoryCategoryID != ItemFilterCategory.Pets)
        {
            Log("Please enter the item Id of a pet for the 5th hotbar slot.", player);
            return;
        }

        if (item.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Consumables or
            ItemFilterCategory.NestedSuperPack)
        {
            var itemModel = new ItemModel()
            {
                ItemId = item.ItemId,
                Count = 0,
                BindingCount = 0,
                DelayUseExpiry = DateTime.Now
            };

            player.Character.Inventory.Items.TryAdd(item.ItemId, itemModel);

            player.SetHotbarSlot(hotbarId - 1, itemModel, ItemRConfig);

            player.SendXt("hs", player.Character.Hotbar);

            return;
        }
        else
        {
            Log("Please enter the item id of a weapon, consumable, or pack.", player);
            return;
        }
    }
}
