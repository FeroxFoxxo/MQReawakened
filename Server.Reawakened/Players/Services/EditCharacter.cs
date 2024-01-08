using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Base.Network.Services;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Players.Events;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Services;

public class EditCharacter(ServerConsole console, EventSink sink,
    ILogger<EditCharacter> logger, UserInfoHandler userInfoHandler,
    AccountHandler accountHandler, CharacterHandler characterHandler, WorldGraph worldGraph,
    ServerRConfig config, NetStateHandler handler, WorldHandler worldHandler,
    ItemCatalog itemCatalog, PlayerEventSink playerEventSink,
    ChatCommands chatCommands) : IService
{
    public void Initialize() => sink.WorldLoad += Load;

    public void Load()
    {
        console.AddCommand(
            "changeName",
            "Changes the name of a requested user's character.",
            NetworkType.Server,
            _ => ChangeCharacterName()
        );

        console.AddCommand(
            "changeLevel",
            "Changes the level of a requested user's character.",
            NetworkType.Server,
            _ => ChangeCharacterLevel()
        );

        console.AddCommand(
            "levelUp",
            "Changes a player's XP level.",
            NetworkType.Server,
            _ => LevelUp()
        );

        console.AddCommand(
            "giveItem",
            "Gives an item to a requested user's character.",
            NetworkType.Server,
            _ => GiveItem()
        );

        console.AddCommand(
            "runCommand",
            "Runs a command for a given player.",
            NetworkType.Server,
            _ => RunPlayerCommand()
        );
    }

    private void RunPlayerCommand()
    {
        Ask.GetCharacter(logger, accountHandler, userInfoHandler, characterHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        if (!handler.IsPlayerOnline(user.Id, out var player))
        {
            logger.LogError("Player must be online to use this command!");
            return;
        }

        logger.LogInformation("Enter command and arguments:");

        var command = Console.ReadLine()?.Trim();

        chatCommands.RunCommand(player, command.Split(' '));
    }

    private void ChangeCharacterName()
    {
        Ask.GetCharacter(logger, accountHandler, userInfoHandler, characterHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        logger.LogInformation("What would you like to set the character '{CharacterName}''s name to?",
            character.Data.CharacterName);

        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            logger.LogError("Character name can not be empty!");
            return;
        }

        character.Data.CharacterName = name;
        user.LastCharacterSelected = name;

        logger.LogInformation("Successfully set character {Id}'s name to {Name}!", character.Id, name);

        logger.LogWarning("Please note this will only apply on next login.");
        playerEventSink.InvokePlayerRefresh();
    }

    private void ChangeCharacterLevel()
    {
        Ask.GetCharacter(logger, accountHandler, userInfoHandler, characterHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        logger.LogInformation("What would you like to set the character '{CharacterName}''s level to?\n",
            character.Data.CharacterName);

        foreach (var levelValue in (Dictionary<string, int>)
                 worldGraph.GetField<WorldGraphXML>("_levelNameToID"))
        {
            if (!File.Exists(Path.Join(config.LevelSaveDirectory, $"{levelValue.Key}.xml")))
                continue;

            var name = worldGraph.GetInfoLevel(levelValue.Value).InGameName;

            logger.LogInformation("    {LevelId}: {InGameLevelName} ({LevelName})",
                levelValue.Value, name, levelValue.Key);
        }

        var level = Console.ReadLine()?.Trim();

        if (!int.TryParse(level, out var levelId))
        {
            logger.LogError("Level ID has to be an integer");
            return;
        }

        character.SetLevel(levelId, 0, logger);

        var levelInfo = worldGraph.GetInfoLevel(levelId);

        var tribe = levelInfo.Tribe;

        if (handler.IsPlayerOnline(user.Id, out var player))
        {
            player.DiscoverTribe(tribe);
            player.SendLevelChange(worldHandler, worldGraph);
        }
        else
        {
            character?.HasAddedDiscoveredTribe(tribe);
            playerEventSink.InvokePlayerRefresh();
        }

        logger.LogInformation(
            "Successfully set character {Id}'s level to {LevelId} '{InGameLevelName}' ({LevelName})!",
            character.Id, levelId, levelInfo.InGameName, levelInfo.Name);
    }

    private void LevelUp()
    {
        Ask.GetCharacter(logger, accountHandler, userInfoHandler, characterHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        logger.LogInformation("Enter experience level:");

        var level = Console.ReadLine()?.Trim();

        if (!int.TryParse(level, out var levelId))
        {
            logger.LogError("Level has to be an integer");
            return;
        }

        if (handler.IsPlayerOnline(user.Id, out var player))
            player.LevelUp(levelId, logger);
        else
            character.SetLevelXp(levelId);
    }

    private void GiveItem()
    {
        Ask.GetCharacter(logger, accountHandler, userInfoHandler, characterHandler, out var character, out var user);

        if (character == null || user == null)
            return;

        logger.LogInformation("Enter item id:");

        var item = Console.ReadLine()?.Trim();

        if (!int.TryParse(item, out var itemId))
        {
            logger.LogError("Item id has to be an integer");
            return;
        }

        logger.LogInformation("Enter Amount:");

        var c = Console.ReadLine()?.Trim();

        if (!int.TryParse(c, out var count))
        {
            logger.LogError("Item count has to be an integer");
            return;
        }

        if (itemCatalog.GetField<ItemHandler>("_itemDescriptionCache") is Dictionary<int, ItemDescription> items &&
            items.TryGetValue(itemId, out var itemDescription))
        {
            character.Data.Inventory.Items.Add(itemDescription.ItemId, new ItemModel
            {
                ItemId = itemDescription.ItemId,
                Count = count,
                BindingCount = 0,
                DelayUseExpiry = DateTime.MinValue
            });

            if (handler.IsPlayerOnline(user.Id, out var player))
                player.SendUpdatedInventory(false);
        }
        else
        {
            logger.LogError("Could not find item with id: '{ItemId}'", itemId);
        }
    }
}
