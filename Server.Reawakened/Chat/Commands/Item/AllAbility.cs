using A2m.Server;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Item;
public class AllAbility : SlashCommand
{
    public override string CommandName => "/AllAbility";

    public override string CommandDescription => "Adds all ability items.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public ItemCatalog ItemCatalog { get; set; }

    public override void Execute(Player player, string[] args)
    {
        foreach (var item in ItemCatalog.Items)
        {
            if (item.Value.SubCategoryId != ItemSubCategory.PassiveAbility)
                continue;

            player.AddItem(item.Value, 1, ItemCatalog);
        }

        player.SendUpdatedInventory();
    }
}
