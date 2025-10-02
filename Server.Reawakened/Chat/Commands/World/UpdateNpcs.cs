using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.World;
public class UpdateNpcs : SlashCommand
{
    public override string CommandName => "/updatenpcs";

    public override string CommandDescription => "Updates all npcs in your current room.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public override void Execute(Player player, string[] args)
    {
        player.UpdateAllNpcsInLevel();
        Log($"All NPCs updated for {player.CharacterName}.", player);
    }
}
