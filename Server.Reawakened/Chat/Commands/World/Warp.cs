using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.World;
public class Warp : SlashCommand
{
    public override string CommandName => "/Warp";

    public override string CommandDescription => "Change's your level to the specified level id.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "levelId",
            Description = "The level id.",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public WorldGraph WorldGraph { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public WorldHandler WorldHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out var levelId))
        {
            Log($"Please specify a valid level id.", player);
            return;
        }

        var levelName = WorldGraph.LevelNameFromID(levelId);
        var levelInfo = WorldGraph.GetInfoLevel(levelName);

        if (args.Length != 2 || levelInfo == null)
        {
            Log($"Please specify a valid level id.", player);
            return;
        }

        if (string.IsNullOrEmpty(levelInfo.Name) || !ServerRConfig.LoadedAssets.Contains(levelInfo.Name))
        {
            Log($"Please specify a valid level.", player);
            return;
        }

        if (!WorldHandler.TryChangePlayerRoom(player, levelInfo.LevelId))
        {
            Log($"Please specify a valid level.", player);
            return;
        }

        Log(
            $"Successfully set character {player.Character.Id}'s level to {levelInfo.LevelId} '{levelInfo.InGameName}' ({levelInfo.Name})",
            player
        );

        Log($"{player.Character.CharacterName} changed to level {levelInfo.LevelId}", player);
    }
}
