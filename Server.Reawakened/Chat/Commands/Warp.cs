using Server.Base.Accounts.Enums;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands;
public class Warp : SlashCommand
{
    public override string CommandName => "/Warp";

    public override string CommandDescription => "Allows you to warp to a new trail.";

    public override List<ParameterModel> Parameters => [
        new ParameterModel() {
            Name = "level",
            Description = "The level id to warp to.",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public WorldGraph WorldGraph { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public WorldHandler WorldHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        var character = player.Character;

        int levelId;

        if (args.Length != 2 || !int.TryParse(args[1], out var level))
        {
            Log($"Please specify a valid level ID.", player);
            return;
        }
        levelId = level;

        var levelInfo = WorldGraph.GetInfoLevel(levelId);

        if (string.IsNullOrEmpty(levelInfo.Name) || !ServerRConfig.LoadedAssets.Contains(levelInfo.Name))
        {
            Log($"Please specify a valid level.", player);
            return;
        }

        if (!WorldHandler.TryChangePlayerRoom(player, levelId))
        {
            Log($"Please specify a valid level.", player);
            return;
        }

        Log(
            $"Successfully set character {character.Id}'s level to {levelId} '{levelInfo.InGameName}' ({levelInfo.Name})",
            player
        );

        Log($"{character.Data.CharacterName} changed to level {levelId}", player);
    }
}
