using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Item;
public class AllItems : SlashCommand
{
    public override string CommandName => "/AllItems";

    public override string CommandDescription => "Adds all items to the player's inventory. (Owner only)";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Owner;

    public ItemCatalog ItemCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        foreach (var item in ItemCatalog.Items)
            player.AddItem(item.Value, 1, ItemCatalog);

        player.SendUpdatedInventory();
    }
}
