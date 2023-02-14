using A2m.Server;
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
using Server.Reawakened.Players.Events;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Services;

public class EditCharacter : IService
{
    private readonly AccountHandler _accountHandler;
    private readonly ServerStaticConfig _config;
    private readonly ServerConsole _console;
    private readonly NetStateHandler _handler;
    private readonly LevelHandler _levelHandler;
    private readonly ILogger<EditCharacter> _logger;
    private readonly EventSink _sink;
    private readonly UserInfoHandler _userInfoHandler;
    private readonly WorldGraph _worldGraph;
    private readonly ItemCatalog _itemCatalog;
    private readonly PlayerEventSink _playerEventSink;

    public EditCharacter(ServerConsole console, EventSink sink,
        ILogger<EditCharacter> logger, UserInfoHandler userInfoHandler,
        AccountHandler accountHandler, WorldGraph worldGraph,
        ServerStaticConfig config, NetStateHandler handler, LevelHandler levelHandler, ItemCatalog itemCatalog, PlayerEventSink playerEventSink)
    {
        _console = console;
        _sink = sink;
        _logger = logger;
        _userInfoHandler = userInfoHandler;
        _accountHandler = accountHandler;
        _worldGraph = worldGraph;
        _config = config;
        _handler = handler;
        _levelHandler = levelHandler;
        _itemCatalog = itemCatalog;
        _playerEventSink = playerEventSink;
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

        _console.AddCommand(new ConsoleCommand("giveItem",
            "Gives an item to a requested user's character.",
            _ => GiveItem()));
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
        _playerEventSink.InvokePlayerRefresh();
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
        
        var tribe = levelInfo.Tribe;

        if (_handler.IsPlayerOnline(user.UserId, out var netState, out var player))
        {
            netState.DiscoverTribe(tribe);
            player.SendLevelChange(netState, _levelHandler, _worldGraph);
        }
        else
        {
            character.Data.HasAddedDiscoveredTribe(tribe);
            _playerEventSink.InvokePlayerRefresh();
        }

        _logger.LogInformation(
            "Successfully set character {Id}'s level to {LevelId} '{InGameLevelName}' ({LevelName})!",
            character.Data.CharacterId, levelId, levelInfo.InGameName, levelInfo.Name);
    }

    private void GiveItem()
    {
        Ask.GetCharacter(_logger, _accountHandler, _userInfoHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        _logger.LogInformation("Enter Item ID:");

        var item = Console.ReadLine()?.Trim();

        if (!int.TryParse(item, out var itemId))
        {
            _logger.LogError("Item ID has to be an integer");
            return;
        }

        _logger.LogInformation("Enter Amount:");

        var c = Console.ReadLine()?.Trim();

        if (!int.TryParse(c, out var count))
        {
            _logger.LogError("Item count has to be an integer");
            return;
        }

        if (_itemCatalog.GetField<ItemHandler>("_itemDescriptionCache") is Dictionary<int, ItemDescription> items &&
            items.TryGetValue(itemId, out var itemDescription))
        {
            character.Data.Inventory.Items.Add(itemDescription.ItemId, new ItemModel
            {
                ItemId = itemDescription.ItemId,
                Count = count,
                BindingCount = 0,
                DelayUseExpiry = DateTime.MinValue
            });

            if (_handler.IsPlayerOnline(user.UserId, out var netState, out _))
                character.SendUpdatedInventory(netState, false);
        }
        else
        {
            _logger.LogError("Could not find item with ID {ItemId}", itemId);
        }
    }
}
