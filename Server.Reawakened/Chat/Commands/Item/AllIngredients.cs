using A2m.Server;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Item;
public class AllIngredients : SlashCommand
{
    public override string CommandName => "/allingredients";

    public override string CommandDescription => "Adds all crafting ingredients and recipes.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "amount",
            Description = "The amount of the item to give or default to 1.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ItemCatalog ItemCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out var amount))
        {
            Log("Invalid amount provided, defaulting to 1...", player);
            amount = 1;
        }

        foreach (var item in ItemCatalog.Items)
        {
            if (item.Value.SubCategoryId is not ItemSubCategory.CoreIngredients
                or ItemSubCategory.Fabrics or ItemSubCategory.Mineral)
                continue;

            player.AddItem(item.Value, amount, ItemCatalog);
        }

        player.SendUpdatedInventory();
    }
}
