using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.World;
public class PlayerPos : SlashCommand
{
    public override string CommandName => "/PlayerPos";

    public override string CommandDescription => "Get the player's current position.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public override void Execute(Player player, string[] args) =>
        Log(player.TempData.Position.ToString(), player);
}
