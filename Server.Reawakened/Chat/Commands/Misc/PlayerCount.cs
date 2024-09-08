using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class PlayerCount : SlashCommand
{
    public override string CommandName => "/playercount";

    public override string CommandDescription => "Check how many player's there are currently.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "detailed",
            Description = "Use true to see all player locations.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2)
        {
            Log($"Currently online players: {player.PlayerContainer.GetAllPlayers().Count}", player);
            return;
        }

        foreach (var item in player.PlayerContainer.GetAllPlayers())
            Log($"{item.CharacterName} - {item.Room.LevelInfo.InGameName} / {item.Room.LevelInfo.LevelId}", player);
    }
}
