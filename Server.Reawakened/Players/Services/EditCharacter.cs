using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Base.Network.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Players.Events;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Services;

public class EditCharacter : IService
{
    private readonly AccountHandler _accountHandler;
    private readonly ServerStaticConfig _config;
    private readonly ServerConsole _console;
    private readonly NetStateHandler _handler;
    private readonly ItemCatalog _itemCatalog;
    private readonly ILogger<EditCharacter> _logger;
    private readonly PlayerEventSink _playerEventSink;
    private readonly EventSink _sink;
    private readonly UserInfoHandler _userInfoHandler;
    private readonly WorldGraph _worldGraph;
    private readonly WorldHandler _worldHandler;

    public EditCharacter(ServerConsole console, EventSink sink,
        ILogger<EditCharacter> logger, UserInfoHandler userInfoHandler,
        AccountHandler accountHandler, WorldGraph worldGraph,
        ServerStaticConfig config, NetStateHandler handler, WorldHandler worldHandler,
        ItemCatalog itemCatalog, PlayerEventSink playerEventSink)
    {
        _console = console;
        _sink = sink;
        _logger = logger;
        _userInfoHandler = userInfoHandler;
        _accountHandler = accountHandler;
        _worldGraph = worldGraph;
        _config = config;
        _handler = handler;
        _worldHandler = worldHandler;
        _itemCatalog = itemCatalog;
        _playerEventSink = playerEventSink;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(
            "changeName",
            "Changes the name of a requested user's character.",
            NetworkType.Server,
            _ => ChangeCharacterName()
        );

        _console.AddCommand(
            "changeLevel",
            "Changes the level of a requested user's character.",
            NetworkType.Server,
            _ => ChangeCharacterLevel()
        );

        _console.AddCommand(
            "levelUp",
            "Changes a player's XP level.",
            NetworkType.Server,
            _ => LevelUp()
        );

        _console.AddCommand(
            "giveItem",
            "Gives an item to a requested user's character.",
            NetworkType.Server,
            _ => GiveItem()
        );
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

        if (!int.TryParse(level, out var levelId))
        {
            _logger.LogError("Level ID has to be an integer");
            return;
        }

        character.SetLevel(levelId, 0, 0, _logger);

        var levelInfo = _worldGraph.GetInfoLevel(levelId);

        var tribe = levelInfo.Tribe;

        if (_handler.IsPlayerOnline(user.UserId, out var netState, out var player))
        {
            netState.DiscoverTribe(tribe);
            player.SendLevelChange(netState, _worldHandler, _worldGraph);
        }
        else
        {
            character.HasAddedDiscoveredTribe(tribe);
            _playerEventSink.InvokePlayerRefresh();
        }

        _logger.LogInformation(
            "Successfully set character {Id}'s level to {LevelId} '{InGameLevelName}' ({LevelName})!",
            character.Data.CharacterId, levelId, levelInfo.InGameName, levelInfo.Name);
    }

    private void LevelUp()
    {
        Ask.GetCharacter(_logger, _accountHandler, _userInfoHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        _logger.LogInformation("Enter experience level:");

        var level = Console.ReadLine()?.Trim();

        if (!int.TryParse(level, out var levelId))
        {
            _logger.LogError("Level has to be an integer");
            return;
        }

        if (_handler.IsPlayerOnline(user.UserId, out _, out var player))
            player.LevelUp(levelId, _logger);
        else
            character.SetLevelXp(levelId);
    }

    private void GiveItem()
    {
        Ask.GetCharacter(_logger, _accountHandler, _userInfoHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        _logger.LogInformation("Enter item id:");

        var item = Console.ReadLine()?.Trim();

        if (!int.TryParse(item, out var itemId))
        {
            _logger.LogError("Item id has to be an integer");
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
            _logger.LogError("Could not find item with id: '{ItemId}'", itemId);
        }
    }
}
