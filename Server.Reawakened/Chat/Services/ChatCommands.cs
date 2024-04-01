using A2m.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Models;
using Server.Base.Accounts.Services;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Worlds.Services;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using System.Text.RegularExpressions;

namespace Server.Reawakened.Chat.Services;

public partial class ChatCommands(
    ItemCatalog itemCatalog, ServerRConfig config, ItemRConfig itemConfig, ILogger<ServerConsole> logger, FileLogger fileLogger,
    WorldHandler worldHandler, InternalAchievement internalAchievement, InternalQuestItem questItem,
    WorldGraph worldGraph, IHostApplicationLifetime appLifetime, AutoSave saves, QuestCatalog questCatalog, 
    CharacterHandler characterHandler, AccountHandler accountHandler) : IService
{
    private readonly Dictionary<string, ChatCommand> commands = [];

    [GeneratedRegex("[^A-Za-z0-9]+")]
    private static partial Regex MyRegex();

    public void Initialize() => appLifetime.ApplicationStarted.Register(RunChatListener);

    public void RunChatListener()
    {
        logger.LogDebug("Setting up chat commands");

        AddCommand(new ChatCommand("setAccess", "owner only", SetAccessLevel));
        AddCommand(new ChatCommand("save", "owner only", SaveLevel));
        AddCommand(new ChatCommand("getAllItems", "[categoryValue] - owner only", GetAllItems));

        AddCommand(new ChatCommand("godMode", string.Empty, GodMode));
        AddCommand(new ChatCommand("maxHP", string.Empty, MaxHealth));

        AddCommand(new ChatCommand("giveItem", "[itemId] [amount]", AddItem));
        AddCommand(new ChatCommand("hotbar", "[hotbarNum] [itemId]", Hotbar));
        AddCommand(new ChatCommand("itemKit", "[itemKit]", ItemKit));
        AddCommand(new ChatCommand("cashKit", "[cashKit]", CashKit));

        AddCommand(new ChatCommand("badgePoints", "[badgePoints]", BadgePoints));
        AddCommand(new ChatCommand("discoverTribes", string.Empty, DiscoverTribes));

        AddCommand(new ChatCommand("changeName", "[first] [middle] [last]", ChangeName));
        AddCommand(new ChatCommand("levelUp", "[newLevel]", LevelUp));
        AddCommand(new ChatCommand("tp", "[X] [Y]", Teleport));
        AddCommand(new ChatCommand("warp", "[levelId]", ChangeLevel));
        AddCommand(new ChatCommand("openDoors", string.Empty, OpenDoors));

        AddCommand(new ChatCommand("closestEntity", string.Empty, ClosestEntity));

        AddCommand(new ChatCommand("getPlayerId", "[id]", GetPlayerId));
        AddCommand(new ChatCommand("playerCount", "[detailed]", PlayerCount));

        AddCommand(new ChatCommand("completeQuest", "[id]", CompleteQuest));
        AddCommand(new ChatCommand("addQuest", "[id]", AddQuest));
        AddCommand(new ChatCommand("findQuest", "[name]", GetQuestByName));

        AddCommand(new ChatCommand("updateNpcs", string.Empty, UpdateLevelNpcs));
        AddCommand(new ChatCommand("playerPos", string.Empty, GetPlayerPos));

        AddCommand(new ChatCommand("resetArmor", "[id]", ResetArmor));

        logger.LogInformation("See chat commands by running {ChatCharStart}help", config.ChatCommandStart);
    }

    public void RunCommand(Player player, string[] args)
    {
        var name = args.FirstOrDefault();

        if (name != null && commands.TryGetValue(name, out var value))
        {
            Log(
                !value.CommandMethod(player, args)
                    ? $"Usage: {config.ChatCommandStart}{value.Name} {value.Arguments}"
                    : "Successfully run command!", player
            );
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

        foreach (var command in commands.Values)
        {
            var padding = config.ChatCommandPadding - command.Name.Length;
            if (padding < 0) padding = 0;

            Log(
                $"  {config.ChatCommandStart}{command.Name.PadRight(padding)}" +
                $"{(command.Arguments.Length > 0 ? $" - {command.Arguments}" : string.Empty)}",
                player
            );
        }
    }

    public void AddCommand(ChatCommand command) => commands.Add(command.Name, command);

    public static bool GetPlayerPos(Player player, string[] args)
    {
        Log($"X: {player.TempData.Position.X}" +
            $" | Y: {player.TempData.Position.Y}" +
            $" | Z: {player.TempData.Position.Z}", player);

        return true;
    }
    public static bool MaxHealth(Player player, string[] args)
    {
        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId, player.Room.Time,
            player.Character.Data.CurrentLife = player.Character.Data.MaxLife, player.Character.Data.MaxLife, player.GameObjectId));

        return true;
    }

    public bool Hotbar(Player player, string[] args)
    {
        player.AddSlots(true);

        if (args.Length <= 2)
            return true;

        if (!int.TryParse(args[1], out var hotbarId) || !int.TryParse(args[2], out var itemId) || hotbarId is < 1 or > 5)
        {
            Log("Please enter a hotbar number from 1-5 and an item Id.", player);
            return false;
        }

        var item = itemCatalog.GetItemFromId(itemId);

        if (item == null)
        {
            Log($"No item with id '{itemId}' could be found.", player);
            return false;
        }

        if (hotbarId == 5 && item.InventoryCategoryID != ItemFilterCategory.Pets)
        {
            Log("Please enter the item Id of a pet for the 5th hotbar slot.", player);
            return false;
        }

        if (item.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Consumables or
            ItemFilterCategory.NestedSuperPack)
        {
            var itemModel = new ItemModel()
            {
                ItemId = item.ItemId,
                Count = 1,
                BindingCount = 1,
                DelayUseExpiry = DateTime.Now
            };

            player.Character.Data.Inventory.Items.TryAdd(item.ItemId, itemModel);

            player.SetHotbarSlot(hotbarId - 1, itemModel, itemCatalog);

            player.SendXt("hs", player.Character.Data.Hotbar);

            return true;
        }
        else
        {
            Log("Please enter the item id of a weapon, consumable, or pack.", player);
            return false;
        }
    }

    public bool GetAllItems(Player player, string[] args)
    {
        if (player.NetState.Get<Account>().AccessLevel < AccessLevel.Owner)
            return false;

        if (args.Length > 1)
        {
            if (!int.TryParse(args[1], out var categoryValue))
                return false;

            if (categoryValue is < 0 or > 12)
            {
                Log($"Please enter a category value between 0-12!", player);
                return false;
            }

            var chosenCategory = itemCatalog.GetItemsDescription((ItemFilterCategory)categoryValue);

            foreach (var item in chosenCategory)
                player.AddItem(item, 1, itemCatalog);
        }
        else
        {
            var categoryList = new List<List<ItemDescription>>();
            for (var i = 0; i < 12; i++)
                categoryList.Add(itemCatalog.GetItemsDescription((ItemFilterCategory)i));

            foreach (var category in categoryList)
                foreach (var item in category)
                    player.AddItem(item, 1, itemCatalog);
        }

        player.SendUpdatedInventory();

        return true;
    }

    private bool GodMode(Player player, string[] args)
    {
        AddKit(player, 1);
        player.AddSlots(true);

        player.AddBananas(config.CashKitAmount, internalAchievement, logger);
        player.AddNCash(config.CashKitAmount);
        player.SendCashUpdate();

        player.LevelUp(config.MaxLevel, logger);
        player.AddPoints();
        player.DiscoverAllTribes();

        player.Character.Data.CurrentLife = player.Character.Data.MaxLife;

        var health = new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time, player.Character.Data.MaxLife, player.Character.Data.MaxLife, "now");
        player.Room.SendSyncEvent(health);

        var heal = new StatusEffect_SyncEvent(player.GameObjectId.ToString(), player.Room.Time, (int)ItemEffectType.Healing, itemConfig.HealAmount, 1, true, player.GameObjectId.ToString(), true);
        player.Room.SendSyncEvent(heal);

        return true;
    }

    private void AddKit(Player player, int amount)
    {
        var items = itemConfig.SingleItemKit
            .Select(itemCatalog.GetItemFromId)
            .ToList();

        foreach (var itemId in itemConfig.StackedItemKit)
        {
            var stackedItem = itemCatalog.GetItemFromId(itemId);

            if (stackedItem == null)
            {
                logger.LogError("Unknown item with id {itemId}", itemId);
                continue;
            }

            for (var i = 0; i < itemConfig.AmountToStack; i++)
                items.Add(stackedItem);
        }

        player.Character.AddKit(items, amount);

        player.SendUpdatedInventory();
    }

    private bool Teleport(Player player, string[] args)
    {
        if (args.Length < 3 || !int.TryParse(args[1], out var xPos) || !int.TryParse(args[2], out var yPos))
        {
            Log("Please enter valid coordinates.", player);
            return false;
        }

        var currentZPosition = player.TempData.Position.Z == 0 ? 0 : 1;

        var zPos = args.Length > 3 ? int.Parse(args[3]) : currentZPosition;

        if (zPos is < 0 or > 1)
            zPos = currentZPosition;

        player.TeleportPlayer(xPos, yPos, Convert.ToInt32(zPos));

        return true;
    }

    private bool DiscoverTribes(Player player, string[] args)
    {
        var character = player.Character;

        player.DiscoverAllTribes();

        Log($"{character.Data.CharacterName} has discovered all tribes!", player);

        return true;
    }

    private bool ItemKit(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length > 2)
            Log($"Unknown kit amount, defaulting to 1", player);

        var amount = 1;

        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out var kitAmount))
                Log($"Invalid kit amount, defaulting to 1", player);

            amount = kitAmount;

            if (amount <= 0)
                amount = 1;
        }

        AddKit(player, amount);

        Log($"{character.Data.CharacterName} received {amount} item kit{(amount > 1 ? "s" : string.Empty)}!", player);

        return true;
    }

    private static bool BadgePoints(Player player, string[] args)
    {
        player.AddPoints();
        return true;
    }

    private bool CashKit(Player player, string[] args)
    {
        var character = player.Character;

        player.AddBananas(config.CashKitAmount, internalAchievement, logger);
        player.AddNCash(config.CashKitAmount);

        Log($"{character.Data.CharacterName} received {config.CashKitAmount} " +
            $"banana{(config.CashKitAmount > 1 ? "s" : string.Empty)} & monkey cash!", player);

        return true;
    }

    private bool ChangeName(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length < 3)
        {
            Log("Please specify a valid name.", player);
            return false;
        }

        var names = args.Select(name =>
            MyRegex().Replace(name.ToLower(), string.Empty)
        ).ToList();

        var firstName = names[1];
        var secondName = names[2];

        var thirdName = names.Count > 3 ? names[3] : string.Empty;

        if (firstName.Length > 0)
            firstName = char.ToUpper(firstName[0]) + firstName[1..];

        if (secondName.Length > 0)
            secondName = char.ToUpper(secondName[0]) + secondName[1..];

        var newName = $"{firstName} {secondName}{thirdName}";

        if (characterHandler.GetCharacterFromName(newName) != null)
        {
            Log("Please specify a name that is not in use by another _player.", player);
            return false;
        }

        character.Data.CharacterName = $"{firstName} {secondName}{thirdName}";

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
            Log($"Please specify a valid level ID.", player);
            return false;
        }

        levelId = level;

        var levelInfo = worldGraph.GetInfoLevel(levelId);

        if (string.IsNullOrEmpty(levelInfo.Name) || !config.LoadedAssets.Contains(levelInfo.Name))
        {
            Log($"Please specify a valid level.", player);
            return false;
        }

        if (!worldHandler.TryChangePlayerRoom(player, levelId))
        {
            Log($"Please specify a valid level.", player);
            return false;
        }

        Log(
            $"Successfully set character {character.Id}'s level to {levelId} '{levelInfo.InGameName}' ({levelInfo.Name})",
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
            Log("Invalid level provided, defaulting to max level...", player);
            level = config.MaxLevel;
        }

        var newLevel = level;

        player.LevelUp(newLevel, logger);

        //If players wanted to level down, it would level them back up to the highest level they've ever hit upon recieving xp.
        //Now their level will stay at the level they set it to.
        character.Data.Reputation = character.Data.ReputationForCurrentLevel;

        Log($"{character.Data.CharacterName} has leveled up to level {newLevel}!", player);

        return true;
    }

    private bool SaveLevel(Player player, string[] args)
    {
        if (player.NetState.Get<Account>().AccessLevel < AccessLevel.Owner)
            return false;

        saves.Save();
        return true;
    }

    private bool AddItem(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length is < 2 or > 3 || !int.TryParse(args[1], out var itemId))
        {
            Log("Please enter a valid level id.", player);
            return false;
        }

        var amount = 1;

        if (args.Length == 3)
        {
            if (!int.TryParse(args[2], out amount))
                Log("Invalid item count, defaulting to 1...", player);

            if (amount <= 0)
                amount = 1;
        }

        var item = itemCatalog.GetItemFromId(itemId);

        if (item == null)
        {
            Log($"No item with id '{itemId}' could be found.", player);
            return false;
        }

        player.AddItem(item, amount, itemCatalog);

        player.SendUpdatedInventory();

        Log($"{character.Data.CharacterName} received {item.ItemName} x{amount}", player);

        return true;
    }

    private bool OpenDoors(Player player, string[] args)
    {
        foreach (var triggerEntity in player.Room.GetEntitiesFromType<TriggerReceiverComp>())
        {
            if (config.IgnoredDoors.Contains(triggerEntity.PrefabName))
                continue;

            triggerEntity.Trigger(true);
        }

        foreach (var vineEntity in player.Room.GetEntitiesFromType<MysticCharmTargetComp>())
            vineEntity.Charm(player);

        return true;
    }

    private bool UpdateLevelNpcs(Player player, string[] args)
    {
        player.UpdateAllNpcsInLevel();
        Log($"All NPCs updated for {player.CharacterName}.", player);
        return true;
    }

    private bool GetPlayerId(Player player, string[] args)
    {
        Log($"{player.CharacterName} has id of {player.GameObjectId}", player);
        return true;
    }

    private bool ClosestEntity(Player player, string[] args)
    {
        var plane = player.GetPlaneEntities();

        var closestGameObjects = plane.Select(gameObject =>
        {
            var x = gameObject.ObjectInfo.Position.X - player.TempData.Position.X;
            var y = gameObject.ObjectInfo.Position.Y - player.TempData.Position.Y;

            var distance = Math.Round(Math.Sqrt(Math.Pow(Math.Abs(x), 2) + Math.Pow(Math.Abs(y), 2)));

            return new Tuple<double, GameObjectModel>(distance, gameObject);
        }).OrderBy(x => x.Item1).ToList();

        if (closestGameObjects.Count == 0)
        {
            Log("No game objects found close to _player!", player);
            return false;
        }

        Log("Closest Game Objects:", player);

        var count = 0;

        if (closestGameObjects.Count > config.MaximumEntitiesToReturnLog)
            closestGameObjects = closestGameObjects.Take(config.MaximumEntitiesToReturnLog).ToList();

        closestGameObjects.Reverse();

        foreach (var item in closestGameObjects)
        {
            if (count > config.MaximumEntitiesToReturnLog)
                break;

            Log($"{item.Item1} units: " +
                $"{item.Item2.ObjectInfo.PrefabName} " +
                $"({item.Item2.ObjectInfo.ObjectId})",
                player);

            count++;
        }

        return true;
    }

    private bool PlayerCount(Player player, string[] args)
    {
        if (args.Length == 1)
            Log($"Currently online players: {player.PlayerContainer.GetAllPlayers().Count}", player);

        if (args.Length == 2)
            foreach (var item in player.PlayerContainer.GetAllPlayers())
                Log($"{item.CharacterName} - {item.Room.LevelInfo.InGameName} / {item.Room.LevelInfo.LevelId}", player);

        return true;
    }

    public QuestDescription GetQuest(Player player, string[] args)
    {
        if (args.Length == 1)
        {
            Log("Please provide a quest id.", player);
            return null;
        }

        if (args.Length != 2)
            return null;

        if (!int.TryParse(args[1], out var questId))
        {
            Log("Please provide a valid quest id.", player);
            return null;
        }

        var questData = questCatalog.GetQuestData(questId);

        if (questData == null)
        {
            Log("Please provide a valid quest id.", player);
            return null;
        }

        if (player.Character.Data.CompletedQuests.Contains(questData.Id))
        {
            Log($"Quest {questData.Name} with id {questData.Id} has been completed already.", player);
            return null;
        }

        return questData;
    }

    private bool CompleteQuest(Player player, string[] args)
    {
        var questData = GetQuest(player, args);

        if (questData == null)
            return false;

        var questModel = player.Character.Data.QuestLog.FirstOrDefault(x => x.Id == questData.Id);

        if (questModel != null)
            player.Character.Data.QuestLog.Remove(questModel);

        player.Character.Data.CompletedQuests.Add(questData.Id);
        Log($"Added quest {questData.Name} with id {questData.Id} to completed quests.", player);

        return true;
    }

    private bool AddQuest(Player player, string[] args)
    {
        var questData = GetQuest(player, args);

        if (questData == null)
            return false;

        var questModel = player.Character.Data.QuestLog.FirstOrDefault(x => x.Id == questData.Id);

        if (questModel != null)
        {
            Log("Quest is already in progress.", player);
            return false;
        }

        player.AddQuest(questData, questItem, config.GameVersion, itemCatalog, fileLogger, "Chat command", logger);
        Log($"Added quest {questData.Name} with id {questData.Id}.", player);

        return true;
    }

    private bool GetQuestByName(Player player, string[] args)
    {
        var name = string.Join(" ", args.Skip(1)).ToLower();

        var closestQuest = questCatalog.QuestCatalogs.FirstOrDefault(q => q.Value.Title.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;

        if (closestQuest == null)
        {
            Log($"Could not find quest with name '{name}'.", player);
            return false;
        }

        Log($"Found quest: '{name}' with ID: '{closestQuest.Id}'.", player);
        return true;
    }

    private bool SetAccessLevel(Player player, string[] args)
    {
        if (args.Length != 3)
            return false;

        if (player.NetState.Get<Account>().AccessLevel < AccessLevel.Owner)
            return false;

        var target = accountHandler.Get(int.Parse(args[1]));

        if (target == null)
        {
            Log("Please provide a valid character id.", player);
            return false;
        }

        target.AccessLevel = (AccessLevel)int.Parse(args[2]);

        Log($"Set {target.Username}'s access level to {target.AccessLevel}", player);

        return true;
    }

    private bool ResetArmor(Player player, string[] args)
    {
        if (args.Length != 2)
            return false;

        if (player.NetState.Get<Account>().AccessLevel < AccessLevel.Moderator)
            return false;

        var target = characterHandler.Get(int.Parse(args[1]));

        if (target == null)
        {
            Log("Please provide a valid character id.", player);
            return false;
        }

        target.Data.Equipment.EquippedItems.Clear();
        target.Data.Equipment.EquippedBinding.Clear();

        Log($"Cleared {target.Data.CharacterName}'s equipped items.", player);

        return true;
    }
}
