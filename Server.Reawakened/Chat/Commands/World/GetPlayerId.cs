using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.World;
public class GetPlayerId : SlashCommand
{
    public override string CommandName => "/getplayerid";

    public override string CommandDescription => "Get your game object id.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public override void Execute(Player player, string[] args) =>
        Log($"{player.CharacterName} has id of {player.GameObjectId}", player);
}
