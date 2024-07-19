using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class LevelUp : SlashCommand
{
    public override string CommandName => "/LevelUp";

    public override string CommandDescription => "Level up to a specified level or default to max level.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "level",
            Description = "Levels the user up to the specified level.",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public WorldStatistics WorldStatistics { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ILogger<LevelUp> Logger { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out var level))
        {
            Log("Invalid level provided, defaulting to max level...", player);
            level = ServerRConfig.MaxLevel;
        }

        player.LevelUp(level, WorldStatistics, ServerRConfig, Logger);

        player.Character.Write.Reputation = player.Character.ReputationForCurrentLevel;
    }
}
