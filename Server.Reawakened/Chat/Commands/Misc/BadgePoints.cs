using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class BadgePoints : SlashCommand
{
    public override string CommandName => "/badgepoints";

    public override string CommandDescription => "Gives 100 of each tribe badge points.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public override void Execute(Player player, string[] args) =>
        player.AddPoints();
}
