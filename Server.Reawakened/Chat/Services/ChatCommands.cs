using A2m.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Services;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using Server.Base.Core.Extensions;
using UnityEngine;

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

    public ChatCommands(ItemCatalog itemCatalog, ServerRConfig config, ILogger<ServerConsole> logger,
        WorldHandler worldHandler, WorldGraph worldGraph, IHostApplicationLifetime appLifetime)
    {
        _itemCatalog = itemCatalog;
        _config = config;
        _logger = logger;
        _worldHandler = worldHandler;
        _worldGraph = worldGraph;
        _appLifetime = appLifetime;
        _commands = new Dictionary<string, ChatCommand>();
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

        _logger.LogInformation("See chat commands by running {ChatCharStart}help", _config.ChatCommandStart);
    }

    public void RunCommand(Player player, string[] args)
    {
        var name = args.FirstOrDefault();

        if (name != null && _commands.TryGetValue(name, out var value))
        {
            if (!value.CommandMethod(player, args))
                Log($"Usage: {_config.ChatCommandStart}{value.Name} {value.Arguments}", player);
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

    var item1 = _itemCatalog.GetItemFromId(394); // Glider.
    var item2 = _itemCatalog.GetItemFromId(395); // Grappling Hook.
    var item3 = _itemCatalog.GetItemFromId(396); // Healing Staff.
    var item4 = _itemCatalog.GetItemFromId(397); // Wooden Slingshot.
    var item5 = _itemCatalog.GetItemFromId(453); // Kernal Blaster.
    var item6 = _itemCatalog.GetItemFromId(2978); // Training Cadet Wooden Sword.
    var item7 = _itemCatalog.GetItemFromId(2883); // Oak Plank Helmet.
    var item8 = _itemCatalog.GetItemFromId(2886); // Oak Plank Armor.
    var item9 = _itemCatalog.GetItemFromId(2880); // Oak Plank Pants.
    var item10 = _itemCatalog.GetItemFromId(1232); // Cat Burglar Mask.
    var item11 = _itemCatalog.GetItemFromId(3152); // Super Monkey Costume Pack.
    var item12 = _itemCatalog.GetItemFromId(3053); // Boom Bomb Construction Kit.
    var item13 = _itemCatalog.GetItemFromId(3023); // LadyBug Warrior Costume Pack.
    var item14 = _itemCatalog.GetItemFromId(3022); // Boom Bug Costume Pack.
    var item15 = _itemCatalog.GetItemFromId(2972); // Ace Pilot Uniform Pack.
    var item16 = _itemCatalog.GetItemFromId(2973); // Crimson Dragon Pack.
    var item17 = _itemCatalog.GetItemFromId(2923); // Banana Box.
    var item18 = _itemCatalog.GetItemFromId(585); // Invisibility Bomb.
    var item19 = _itemCatalog.GetItemFromId(1568); // Shiny Red Apple.
    var item20 = _itemCatalog.GetItemFromId(1704); // Healing Potion.

    var items = new List<ItemDescription>
    {
        item1,
        item2,
        item3,
        item4,
        item5,
        item6,
        item7,
        item8,
        item9,
        item10,
        item11,
        item12,
        item13,
        item14,
        item15,
        item16,
        item17,
        item18,
        item19,
        item20
    };

    for (var i = 0; i < 98; i++)
    {
        items.Add(item3);
        items.Add(item18);
        items.Add(item19);
        items.Add(item20);
    }

    character.AddKit(items, amount);

    player.SendUpdatedInventory(false);

    if (amount > 1)
    {
        Log($"{character.Data.CharacterName} received {amount} item kits!", player);
    }
    else
    {
        Log($"{character.Data.CharacterName} received {amount} item kit!", player);
    }

    return true;
}

    public static bool Cash(Player player, string[] args)
    {

        var character = player.Character;

        var cashAmount = Convert.ToInt32(args[1]);

        player.AddBananas(cashAmount);

        Log($"{character.Data.CharacterName} received {cashAmount} bananas!", player);

        return true;

    }

    public static bool MCash(Player player, string[] args)
    {

        var character = player.Character;

        var cashAmount = Convert.ToInt32(args[1]);
  
        player.AddMCash(cashAmount);

        Log($"{character.Data.CharacterName} received {cashAmount} Monkey Cash!", player);

        return true;

    }

    public static bool BadgePoints(Player player, string[] args)
    {

        var character = player.Character;

        var amount = Convert.ToInt32(args[1]);

        player.AddPoints(amount);

        Log($"{character.Data.CharacterName} received {amount} badge points!", player);

        return true;

    }

    public static bool CashKit(Player player, string[] args)
    {

        var character = player.Character;
  
        var cashKitAmount = 100000;

        player.AddBananas(cashKitAmount);
        player.AddMCash(cashKitAmount);

        Log($"{character.Data.CharacterName} received {cashKitAmount} Bananas & Monkey Cash!", player);

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

        var name = args[1].ToLower(); // Convert to lowercase
        var name2 = args[2].ToLower(); // Convert to lowercase
        var name3 = args.Length > 3 ? args[3].ToLower() : ""; // Convert to lowercase, handle the case when args[3] is missing

        // Ensure the first letter of name and name2 are uppercase
        if (name.Length > 0)
        {
            name = char.ToUpper(name[0]) + name.Substring(1);
        }

        if (name2.Length > 0)
        {
            name2 = char.ToUpper(name2[0]) + name2.Substring(1);
        }

        character.Data.CharacterName = name + " " + name2 + name3;

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
