using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.World;
public class DiscoverTribes : SlashCommand
{
    public override string CommandName => "/discovertribes";

    public override string CommandDescription => "Discovers all tribes to unlock the map/quest log.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public override void Execute(Player player, string[] args)
    {
        player.DiscoverAllTribes();

        Log($"{player.Character.CharacterName} has discovered all tribes!", player);
    }
}
