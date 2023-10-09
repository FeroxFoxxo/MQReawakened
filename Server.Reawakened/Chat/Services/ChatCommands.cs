using A2m.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Services;
using Server.Base.Worlds.Services;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using System.Text.RegularExpressions;

namespace Server.Reawakened.Chat.Services;

public class ChatCommands : IService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly Dictionary<string, ChatCommand> _commands;
    private readonly ServerRConfig _config;
    private readonly ItemCatalog _itemCatalog;
    private readonly ILogger<ServerConsole> _logger;
    private readonly WorldGraph _worldGraph;
    private readonly WorldHandler _worldHandler;
    private readonly AutoSave _saves;

    public ChatCommands(ItemCatalog itemCatalog, ServerRConfig config, ILogger<ServerConsole> logger,
        WorldHandler worldHandler, WorldGraph worldGraph, IHostApplicationLifetime appLifetime, AutoSave saves)
    {
        _itemCatalog = itemCatalog;
        _config = config;
        _logger = logger;
        _worldHandler = worldHandler;
        _worldGraph = worldGraph;
        _appLifetime = appLifetime;
        _saves = saves;
        _commands = [];
    }

    public void Initialize() => _appLifetime.ApplicationStarted.Register(RunChatListener);

    public void RunChatListener()
    {
        _logger.LogDebug("Setting up chat commands");

        AddCommand(new ChatCommand("warp", "[levelId]", ChangeLevel));
        AddCommand(new ChatCommand("giveItem", "[itemId] [amount]", AddItem));
        AddCommand(new ChatCommand("levelUp", "[newLevel]", LevelUp));
        AddCommand(new ChatCommand("itemKit", "[itemKit]", ItemKit));
        AddCommand(new ChatCommand("bananas", "[bananaAmount]", Cash));
        AddCommand(new ChatCommand("cash", "[cashAmount]", MCash));
        AddCommand(new ChatCommand("cashKit", "[cashKit]", CashKit));
        AddCommand(new ChatCommand("changeName", "[name] [name2] [name3]", ChangeName));
        AddCommand(new ChatCommand("badgePoints", "[amount]", BadgePoints));
        AddCommand(new ChatCommand("save", "", SaveLevel));

        _logger.LogInformation("See chat commands by running {ChatCharStart}help", _config.ChatCommandStart);
    }

    public void RunCommand(Player player, string[] args)
    {
        var name = args.FirstOrDefault();

        if (name != null && _commands.TryGetValue(name, out var value))
        {
            Log(
                !value.CommandMethod(player, args)
                    ? $"Usage: {_config.ChatCommandStart}{value.Name} {value.Arguments}"
                    : "Successfully run command!", player);
        }
        else
        {
            DisplayHelp(player);
        }
    }

    private static void Log(string logMessage, Player player) =>
        player.Chat(CannedChatChannel.Tell, "Console", logMessage);

    public void DisplayHelp(Player player)
    {
        Log("Chat Commands:", player);

        foreach (var command in _commands.Values)
        {
            var padding = _config.ChatCommandPadding - command.Name.Length;
            if (padding < 0) padding = 0;

            Log(
                $"  {_config.ChatCommandStart}{command.Name.PadRight(padding)}" +
                $"{(command.Arguments.Length > 0 ? $" - {command.Arguments}" : "")}",
                player
            );
        }
    }

    public void AddCommand(ChatCommand command) => _commands.Add(command.Name, command);

    public bool ItemKit(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length is < 1 or > 2)
            return false;

        var amount = 1;

        if (args.Length == 2)
        {
            amount = Convert.ToInt32(args[1]);

            if (amount <= 0)
                amount = 1;
        }

        var items = _config.SingleItemKit
            .Select(itemId => _itemCatalog.GetItemFromId(itemId))
            .ToList();

        foreach (var itemId in _config.StackedItemKit)
        {
            var stackedItem = _itemCatalog.GetItemFromId(itemId);

            for (var i = 0; i < _config.AmountToStack; i++)
                items.Add(stackedItem);
        }

        character.AddKit(items, amount);

        player.SendUpdatedInventory(false);

        Log($"{character.Data.CharacterName} received {amount} item kit{(amount > 1 ? "s" : "")}!", player);

        return true;
    }

    public static bool Cash(Player player, string[] args)
    {
        var character = player.Character;

        var cashAmount = Convert.ToInt32(args[1]);
        player.AddBananas(cashAmount);

        Log($"{character.Data.CharacterName} received {cashAmount} banana{(cashAmount > 1 ? "s" : "")}!", player);

        return true;
    }

    public static bool MCash(Player player, string[] args)
    {
        var character = player.Character;

        var cashAmount = Convert.ToInt32(args[1]);

        player.AddMCash(cashAmount);

        Log($"{character.Data.CharacterName} received {cashAmount} monkey cash!", player);

        return true;
    }

    public static bool BadgePoints(Player player, string[] args)
    {
        var character = player.Character;

        var amount = Convert.ToInt32(args[1]);

        player.AddPoints(amount);

        Log($"{character.Data.CharacterName} received {amount} badge point{(amount > 1 ? "s" : "")}!", player);

        return true;
    }

    public bool CashKit(Player player, string[] args)
    {
        var character = player.Character;

        player.AddBananas(_config.CashKitAmount);
        player.AddMCash(_config.CashKitAmount);

        Log($"{character.Data.CharacterName} received {_config.CashKitAmount} " +
            $"banana{(_config.CashKitAmount > 1 ? "s" : "")} & monkey cash!", player);

        return true;
    }

    public static bool ChangeName(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length < 3)
        {
            Log("Error: Invalid Input!", player);
            return false;
        }

        var names = args.Select(name =>
            Regex.Replace(name.ToLower(), "[^A-Za-z0-9]+", "")
        ).ToList();

        var firstName = names[1];
        var secondName = names[2];

        var thirdName = names.Count > 3 ? names[3] : string.Empty;

        if (firstName.Length > 0)
            firstName = char.ToUpper(firstName[0]) + firstName[1..];

        if (secondName.Length > 0)
            secondName = char.ToUpper(secondName[0]) + secondName[1..];

        character.Data.CharacterName = $"{firstName} {secondName}{thirdName}";

        Log($"You have changed your monkey's name to {character.Data.CharacterName}!", player);
        Log("This change will apply only once you've logged out.", player);

        return true;
    }

    private bool ChangeLevel(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length != 2)
            return false;

        var levelId = Convert.ToInt32(args[1]);

        character.SetLevel(levelId, _logger);

        var levelInfo = _worldGraph.GetInfoLevel(levelId);

        var tribe = levelInfo.Tribe;

        player.DiscoverTribe(tribe);
        player.SendLevelChange(_worldHandler, _worldGraph);

        Log(
            $"Successfully set character {character.Data.CharacterId}'s level to {levelId} '{levelInfo.InGameName}' ({levelInfo.Name})",
            player
        );

        Log($"{character.Data.CharacterName} changed to level {levelId}", player);

        return true;
    }

    private bool LevelUp(Player player, string[] args)
    {
        if (args.Length != 2)
            return false;

        var newLevel = Convert.ToInt32(args[1]);
        player.LevelUp(newLevel, _logger);

        return true;
    }

    private bool SaveLevel(Player player, string[] args)
    {
        _saves.Save();
        return true;
    }

    private bool AddItem(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length is < 2 or > 3)
            return false;

        var itemId = Convert.ToInt32(args[1]);
        var amount = 1;

        if (args.Length == 3)
        {
            amount = Convert.ToInt32(args[2]);

            if (amount <= 0)
                amount = 1;
        }

        var item = _itemCatalog.GetItemFromId(itemId);

        if (item == null)
        {
            Log($"Can't find item with id {itemId}", player);
            return false;
        }

        character.AddItem(item, amount);

        player.SendUpdatedInventory(false);

        Log($"{character.Data.CharacterName} received {item.ItemName} x{amount}", player);

        return true;
    }
}
