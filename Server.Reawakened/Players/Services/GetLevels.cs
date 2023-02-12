using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Services;

public class GetLevels : IService
{
    private readonly ServerConsole _serverConsole;
    private readonly ILogger<GetLevels> _logger;
    private readonly ServerConfig _config;
    private readonly WorldGraph _worldGraph;

    public GetLevels(ServerConsole serverConsole, ILogger<GetLevels> logger,
        ServerConfig config, WorldGraph worldGraph)
    {
        _serverConsole = serverConsole;
        _logger = logger;
        _config = config;
        _worldGraph = worldGraph;
    }

    public void Initialize() =>
        _serverConsole.AddCommand(new ConsoleCommand("printLevels",
            "Prints out all the levels in the world graph.", PrintLevels));

    private void PrintLevels(string[] command)
    {
        var shouldFilter = _logger.Ask(
            "Would you like the levels filtered to only the ones that you have available to visit?",
            true);

        _logger.LogInformation("Levels:");

        foreach (var levelValue in (Dictionary<string, int>)
                 _worldGraph.GetField<WorldGraphXML>("_levelNameToID"))
        {
            if (shouldFilter)
                if (!File.Exists(Path.Join(_config.LevelSaveDirectory, $"{levelValue.Key}.xml")))
                    continue;

            var name = _worldGraph.GetInfoLevel(levelValue.Value).InGameName;

            _logger.LogInformation("    {LevelId}: {InGameLevelName} ({LevelName})",
                levelValue.Value, name, levelValue.Key);
        }
    }
}
