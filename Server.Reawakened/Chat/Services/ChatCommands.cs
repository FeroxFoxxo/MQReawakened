using A2m.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Services;
using Server.Base.Worlds;
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
    private readonly World _world;

    public ChatCommands(ItemCatalog itemCatalog, ServerRConfig config, ILogger<ServerConsole> logger,
        WorldHandler worldHandler, WorldGraph worldGraph, IHostApplicationLifetime appLifetime, World world)
    {
        _itemCatalog = itemCatalog;
        _config = config;
        _logger = logger;
        _worldHandler = worldHandler;
        _worldGraph = worldGraph;
        _appLifetime = appLifetime;
        _world = world;
        _commands = new Dictionary<string, ChatCommand>();
    }

    public void Initialize() => _appLifetime.ApplicationStarted.Register(RunChatListener);

    public void RunChatListener()
    {
        _logger.LogDebug("Setting up chat commands");

        AddCommand(new ChatCommand("changeName", "[name] [name2] [name3]", ChangeName));
        AddCommand(new ChatCommand("hotbar", "[petSlot (1 = true)]", AddHotbar));
        AddCommand(new ChatCommand("giveItem", "[itemId] [amount]", AddItem));
        AddCommand(new ChatCommand("badgePoints", "[amount]", BadgePoints));
        AddCommand(new ChatCommand("levelUp", "[newLevel]", LevelUp));
        AddCommand(new ChatCommand("itemKit", "[itemKit]", ItemKit));
        AddCommand(new ChatCommand("cashKit", "[cashKit]", CashKit));
        AddCommand(new ChatCommand("warp", "[levelId]", ChangeLevel));
        AddCommand(new ChatCommand("map", "", DiscoverMap));
        AddCommand(new ChatCommand("save", "", SaveLevel));

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

    private bool DiscoverMap(Player player, string[] args)
    {
        var character = player.Character;

        player.DiscoverAllTribes();

        Log($"{character.Data.CharacterName} has discovered all tribes! (view map)", player);

        return true;
    }

    private bool AddHotbar(Player player, string[] args)
    {
        var pet = false;

        var petSlot = 0;

        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out var petSlotValue))
                Log("Invalid input, defaulting to 0 (false)", player);

            if (petSlotValue < 0 || petSlotValue > 1)
                Log("Input out of range, defaulting to 0 (false)", player);

            else
                petSlot = petSlotValue;

            if (petSlot == 0)
            {
                pet = false;
                Log("Adding slots (no pet slot)", player);
            }
            else if (petSlot == 1)
            {
                pet = true;
                Log("Adding slots (pet slot)", player);
            }
        }

        player.AddSlots(pet);

        Log("Hotbar has been setup! (slots apply once logged out)", player);

        return true;
    }


    private bool ItemKit(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length > 2)
            Log($"Invalid input, defaulting to 1", player);

        var amount = 1;

        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out var kitAmount))
                Log($"Invalid input, defaulting to 1", player);

            amount = kitAmount;

            if (amount <= 0)
                amount = 1;
        }

        var glider = _itemCatalog.GetItemFromId(394);
        var grapplingHook = _itemCatalog.GetItemFromId(395);
        var healingStaff = _itemCatalog.GetItemFromId(396);
        var woodenSlingshot = _itemCatalog.GetItemFromId(397);
        var kernelBlaster = _itemCatalog.GetItemFromId(453);
        var woodenSword = _itemCatalog.GetItemFromId(2978);
        var oakHelmet = _itemCatalog.GetItemFromId(2883);
        var oakArmor = _itemCatalog.GetItemFromId(2886);
        var oakPants = _itemCatalog.GetItemFromId(2880);
        var burglarMask = _itemCatalog.GetItemFromId(1232);
        var superMonkey = _itemCatalog.GetItemFromId(3152);
        var boomBomb = _itemCatalog.GetItemFromId(3053);
        var warriorCostume = _itemCatalog.GetItemFromId(3023);
        var boomBug = _itemCatalog.GetItemFromId(3022);
        var acePilot = _itemCatalog.GetItemFromId(2972);
        var crimsonDragon = _itemCatalog.GetItemFromId(2973);
        var bananaBox = _itemCatalog.GetItemFromId(2923);
        var invisibleBomb = _itemCatalog.GetItemFromId(585);
        var redApple = _itemCatalog.GetItemFromId(1568);
        var healingPotion = _itemCatalog.GetItemFromId(1704);

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

    private static bool BadgePoints(Player player, string[] args)
    {
        var character = player.Character;

        int amount;

        if (args.Length < 2)
        {
            Log($"Invalid, enter number of badge points", player);
            return false;
        }
        if (!int.TryParse(args[1], out var pointsAmount) || args.Length < 2)
            Log($"Invalid input, defaulting to 1", player);

        amount = pointsAmount;

        if (amount <= 0)
            amount = 1;

        player.AddPoints(amount);

        Log(
            amount > 1
                ? $"{character.Data.CharacterName} received {amount} badge points!"
                : $"{character.Data.CharacterName} received {amount} badge point! (get grinding noob)", player
           );

        return true;
    }


    public static bool CashKit(Player player, string[] args)
    {
        var character = player.Character;

        const int cashKitAmount = 69420;

        player.AddBananas(cashKitAmount);
        player.AddMCash(cashKitAmount);

        return true;
    }

    public static bool ChangeName(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length < 3)
        {
            Log("Error: Invalid input!", player);
            return false;
        }

        var first = args[1].ToLower();
        var middle = args[2].ToLower();

        var last = args.Length > 3 ? args[3].ToLower() : "";

        if (first.Length > 0)
            first = char.ToUpper(first[0]) + first[1..];

        if (middle.Length > 0)
            middle = char.ToUpper(middle[0]) + middle[1..];

        character.Data.CharacterName = first + " " + middle + last;

        Log($"You have changed your monkey's name to {character.Data.CharacterName}!", player);
        Log("This change will apply only once you've logged out.", player);

        return true;
    }

    private bool ChangeLevel(Player player, string[] args)
    {
        var character = player.Character;

        int levelId;
        if (args.Length != 2 || !int.TryParse(args[1], out var level))
        {
            Log($"Invalid input, enter level ID", player);
            return false;
        }

        levelId = level;

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
        var character = player.Character;

        if (args.Length != 2 || !int.TryParse(args[1], out var level))
        {
            Log("Invalid input, defaulting to max level...", player);
            level = 65;
        }

        var newLevel = level;

        player.LevelUp(newLevel, _logger);

        Log($"{character.Data.CharacterName} has leveled up to level {newLevel}!", player);

        return true;
    }


    private bool SaveLevel(Player player, string[] args)
    {
        _world.Save(false, true);
        return true;
    }

    private bool AddItem(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length is < 2 or > 3 || !int.TryParse(args[1], out var itemId))
        {
            Log("Invalid input, enter item ID", player);
            return false;
        }

        var amount = 1;

        if (args.Length == 3)
        {
            if (!int.TryParse(args[2], out amount))
                Log("Invalid amount, defaulting to 1", player);

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
