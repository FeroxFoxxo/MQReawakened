using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.World;
public class ChangeLevel : SlashCommand
{
    public override string CommandName => "/changelevel";

    public override string CommandDescription => "Allows you to warp to a new level.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "levelName",
            Description = "The level name to warp to.",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public WorldGraph WorldGraph { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public WorldHandler WorldHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        var levelInfo = WorldGraph.GetInfoLevel(args[1]);

        if (args.Length != 2 || levelInfo == null)
        {
            Log($"Please specify a valid level name.", player);
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
