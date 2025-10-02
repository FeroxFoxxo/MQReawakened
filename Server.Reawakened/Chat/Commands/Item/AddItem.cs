using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Item;
public class AddItem : SlashCommand
{
    public override string CommandName => "/additem";

    public override string CommandDescription => "Adds an item using it's prefab name.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "name",
            Description = "The prefab name of the item to be added.",
            Optional = false
        },
        new ParameterModel()
        {
            Name = "amount",
            Description = "The amount of the item to give or default to 1.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public ItemCatalog ItemCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        var item = ItemCatalog.GetItemFromPrefabName(args[1]);

        if (item == null)
        {
            Log($"No item with prefab name '{args[1]}' could be found.", player);
            return;
        }

        if (args.Length < 3 || !int.TryParse(args[2], out var amount))
        {
            Log("Invalid amount provided, defaulting to 1...", player);
            amount = 1;
        }

        player.AddItem(item, amount, ItemCatalog);
        player.SendUpdatedInventory();
    }
}
