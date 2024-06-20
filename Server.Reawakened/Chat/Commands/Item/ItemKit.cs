using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Item;
public class ItemKit : SlashCommand
{
    public override string CommandName => "/ItemKit";

    public override string CommandDescription => "This will give an item kit.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "amount",
            Description = "The amount of the kit to give.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ItemRConfig ItemRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<ItemKit> Logger { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length > 2)
            Log($"Unknown kit amount, defaulting to 1", player);

        var amount = 1;

        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out var kitAmount))
                Log($"Invalid kit amount, defaulting to 1", player);

            amount = kitAmount;

            if (amount <= 0)
                amount = 1;
        }

        AddKit(player, amount);

        Log($"{player.Character.CharacterName} received {amount} item kit{(amount > 1 ? "s" : string.Empty)}!", player);
    }

    private void AddKit(Player player, int amount)
    {
        var items = ItemRConfig.SingleItemKit
            .Select(ItemCatalog.GetItemFromId)
        .ToList();

        foreach (var itemId in ItemRConfig.StackedItemKit)
        {
            var stackedItem = ItemCatalog.GetItemFromId(itemId);

            if (stackedItem == null)
            {
                Logger.LogError("Unknown item with id {itemId}", itemId);
                continue;
            }

            for (var i = 0; i < ItemRConfig.AmountToStack; i++)
                items.Add(stackedItem);
        }

        player.Character.AddKit(items, amount);

        player.SendUpdatedInventory();
    }
}
