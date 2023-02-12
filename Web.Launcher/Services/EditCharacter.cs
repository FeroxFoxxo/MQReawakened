using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Network.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Web.Launcher.Services;

public class EditCharacter : IService
{
    private readonly AccountHandler _accountHandler;
    private readonly ServerConfig _config;
    private readonly ServerConsole _console;
    private readonly StartGame _game;
    private readonly NetStateHandler _handler;
    private readonly LevelHandler _levelHandler;
    private readonly ILogger<EditCharacter> _logger;
    private readonly EventSink _sink;
    private readonly UserInfoHandler _userInfoHandler;
    private readonly WorldGraph _worldGraph;

    public EditCharacter(ServerConsole console, EventSink sink,
        ILogger<EditCharacter> logger, UserInfoHandler userInfoHandler,
        AccountHandler accountHandler, WorldGraph worldGraph,
        ServerConfig config, StartGame game, NetStateHandler handler, LevelHandler levelHandler)
    {
        _console = console;
        _sink = sink;
        _logger = logger;
        _userInfoHandler = userInfoHandler;
        _accountHandler = accountHandler;
        _worldGraph = worldGraph;
        _config = config;
        _game = game;
        _handler = handler;
        _levelHandler = levelHandler;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(new ConsoleCommand("changeName",
            "Changes the name of a requested user's character.",
            _ => ChangeCharacterName()));

        _console.AddCommand(new ConsoleCommand("changeLevel",
            "Changes the level of a requested user's character.",
            _ => ChangeCharacterLevel()));
    }

    private void ChangeCharacterName()
    {
        Ask.GetCharacter(_logger, _accountHandler, _userInfoHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        _logger.LogInformation("What would you like to set the character '{CharacterName}''s name to?",
            character.Data.CharacterName);

        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Character name can not be empty!");
            return;
        }

        character.Data.CharacterName = name;
        user.LastCharacterSelected = name;

        _logger.LogInformation("Successfully set character {Id}'s name to {Name}!", character.Data.CharacterId, name);

        _logger.LogWarning("Please note this will only apply on next login.");
        _game.AskIfRestart();
    }

    private void ChangeCharacterLevel()
    {
        Ask.GetCharacter(_logger, _accountHandler, _userInfoHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        _logger.LogInformation("What would you like to set the character '{CharacterName}''s level to?\n",
            character.Data.CharacterName);

        foreach (var levelValue in (Dictionary<string, int>)
                 _worldGraph.GetField<WorldGraphXML>("_levelNameToID"))
        {
            if (!File.Exists(Path.Join(_config.LevelSaveDirectory, $"{levelValue.Key}.xml")))
                continue;

            var name = _worldGraph.GetInfoLevel(levelValue.Value).InGameName;

            _logger.LogInformation("    {LevelId}: {InGameLevelName} ({LevelName})",
                levelValue.Value, name, levelValue.Key);
        }

        var level = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(level))
        {
            _logger.LogError("Character's level can not be empty!");
            return;
        }

        if (!int.TryParse(level, out var levelId))
        {
            _logger.LogError("Character's level has to be an integer!");
            return;
        }

        character.SetCharacterSpawn(0, 0, _logger);

        character.Level = levelId;

        var levelInfo = _worldGraph.GetInfoLevel(levelId);

        _logger.LogInformation("Successfully set character {Id}'s level to {LevelId} '{InGameLevelName}' ({LevelName})!",
            character.Data.CharacterId, levelId, levelInfo.InGameName, levelInfo.Name);

        var tribe = levelInfo.Tribe;

        if (_handler.IsPlayerOnline(user.UserId, out var netState, out var player))
        {
            netState.DiscoverTribe(tribe);
            player.SendLevelChange(netState, _levelHandler, _worldGraph);
        }
        else
        {
            character.Data.HasAddedDiscoveredTribe(tribe);
            _game.AskIfRestart();
        }
    }
}
