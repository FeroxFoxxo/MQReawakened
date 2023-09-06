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
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

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

        var glider = _itemCatalog.GetItemFromId(394); // Glider.
        var grapplingHook = _itemCatalog.GetItemFromId(395); // Grappling Hook.
        var healingStaff = _itemCatalog.GetItemFromId(396); // Healing Staff.
        var woodenSlingshot = _itemCatalog.GetItemFromId(397); // Wooden Slingshot.
        var kernelBlaster = _itemCatalog.GetItemFromId(453); // Kernel Blaster.
        var woodenSword = _itemCatalog.GetItemFromId(2978); // Training Cadet Wooden Sword.
        var oakHelmet = _itemCatalog.GetItemFromId(2883); // Oak Plank Helmet.
        var oakArmor = _itemCatalog.GetItemFromId(2886); // Oak Plank Armor.
        var oakPants = _itemCatalog.GetItemFromId(2880); // Oak Plank Pants.
        var burglarMask = _itemCatalog.GetItemFromId(1232); // Cat Burglar Mask.
        var superMonkey = _itemCatalog.GetItemFromId(3152); // Super Monkey Costume Pack.
        var boomBomb = _itemCatalog.GetItemFromId(3053); // Boom Bomb Construction Kit.
        var warriorCostume = _itemCatalog.GetItemFromId(3023); // LadyBug Warrior Costume Pack.
        var boomBug = _itemCatalog.GetItemFromId(3022); // Boom Bug Costume Pack.
        var acePilot = _itemCatalog.GetItemFromId(2972); // Ace Pilot Uniform Pack.
        var crimsonDragon = _itemCatalog.GetItemFromId(2973); // Crimson Dragon Pack.
        var bananaBox = _itemCatalog.GetItemFromId(2923); // Banana Box.
        var invisibleBomb = _itemCatalog.GetItemFromId(585); // Invisibility Bomb.
        var redApple = _itemCatalog.GetItemFromId(1568); // Shiny Red Apple.
        var healingPotion = _itemCatalog.GetItemFromId(1704); // Healing Potion.

        var items = new List<ItemDescription>
        {
            glider,
            grapplingHook,
            healingStaff,
            woodenSlingshot,
            kernelBlaster,
            woodenSword,
            oakHelmet,
            oakArmor,
            oakPants,
            burglarMask,
            superMonkey,
            boomBomb,
            warriorCostume,
            boomBug,
            acePilot,
            crimsonDragon,
            bananaBox,
            invisibleBomb,
            redApple,
            healingPotion
        };

        const int totalCount = 98;

        for (var i = 0; i < totalCount; i++)
        {
            items.Add(healingStaff);
            items.Add(invisibleBomb);
            items.Add(redApple);
            items.Add(healingPotion);
        }

        character.AddKit(items, amount);

        player.SendUpdatedInventory(false);

        Log(
            amount > 1
                ? $"{character.Data.CharacterName} received {amount} item kits!"
                : $"{character.Data.CharacterName} received {amount} item kit!", player
        );

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

        const int cashKitAmount = 100000;

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

        // Convert to lowercase, handle the case when args[3] is missing
        var name3 = args.Length > 3 ? args[3].ToLower() : "";

        // Ensure the first letter of name and name2 are uppercase
        if (name.Length > 0)
            name = char.ToUpper(name[0]) + name[1..];

        if (name2.Length > 0)
            name2 = char.ToUpper(name2[0]) + name2[1..];

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
