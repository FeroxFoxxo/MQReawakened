using A2m.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Chat.Services;

public class ChatCommands : IService
{
    private readonly Dictionary<string, ChatCommand> _commands;
    private readonly ItemCatalog _itemCatalog;
    private readonly ServerStaticConfig _config;
    private readonly ILogger<ServerConsole> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly LevelHandler _levelHandler;
    private readonly WorldGraph _worldGraph;

    public ChatCommands(ItemCatalog itemCatalog, ServerStaticConfig config, ILogger<ServerConsole> logger, EventSink eventSink, LevelHandler levelHandler, WorldGraph worldGraph, IHostApplicationLifetime appLifetime)
    {
        _itemCatalog = itemCatalog;
        _config = config;
        _logger = logger;
        _levelHandler = levelHandler;
        _worldGraph = worldGraph;
        _appLifetime = appLifetime;
        _commands = new Dictionary<string, ChatCommand>();
    }

    public void Initialize() => _appLifetime.ApplicationStarted.Register(RunChatListener);

    public void RunChatListener()
    {
        _logger.LogInformation("Setting Up Chat Commands");

        AddCommand(new ChatCommand("listlevels", "[filterUnknown](true/false)", ListLevels));
        AddCommand(new ChatCommand("level", "[levelId]", ChangeLevel));
        AddCommand(new ChatCommand("item", "[itemId] [amount]", AddItem));

        _logger.LogDebug("See chat commands by running {ChatCharStart}help", _config.ChatCommandStart);
    }
    
    public void RunCommand(NetState netState, string[] args)
    {
        var name = args.FirstOrDefault();

        if (name != null && _commands.TryGetValue(name, out var value))
            Log(value.CommandMethod(netState, args)
                    ? $"Successfully ran command '{name}'"
                    : $"Usage: {_config.ChatCommandStart}{value.Name} {value.Arguments}",
                netState);
        else
            DisplayHelp(netState);
    }

    private static void Log(string logMessage, NetState netState) =>
        netState.Chat(CannedChatChannel.Tell, "Console", logMessage);

    private void DisplayHelp(NetState netState)
    {
        Log("Chat Commands:", netState);

        foreach (var command in _commands.Values)
        {
            var padding = _config.ChatCommandPadding - command.Name.Length;
            if (padding < 0) padding = 0;

            Log(
                $"  {_config.ChatCommandStart}{command.Name.PadRight(padding)}" +
                $"{(command.Arguments.Length > 0 ? $" - {command.Arguments}" : "")}",
                netState
            );
        }
    }

    public void AddCommand(ChatCommand command) => _commands.Add(command.Name, command);

    private bool ChangeLevel(NetState netState, string[] args)
    {
        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();

        if (args.Length != 2)
            return false;

        var levelId = Convert.ToInt32(args[1]);

        character.SetCharacterSpawn(0, 0, _logger);

        character.Level = levelId;
        var levelInfo = _worldGraph.GetInfoLevel(levelId);

        var tribe = levelInfo.Tribe;

        netState.DiscoverTribe(tribe);
        player.SendLevelChange(netState, _levelHandler, _worldGraph);

        Log(
            $"Successfully set character {character.Data.CharacterId}'s level to {levelId} '{levelInfo.InGameName}' ({levelInfo.Name})",
            netState
        );

        Log($"{character.Data.CharacterName} changed to level {levelId}", netState);

        return true;
    }

    private bool AddItem(NetState netState, string[] args)
    {
        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();

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
            Log($"Can't find item with id {itemId}", netState);
            return false;
        }

        character.AddItem(item, amount);
        character.SendUpdatedInventory(netState, false);

        Log($"{character.Data.CharacterName} received {item.ItemName} x{amount}", netState);

        return true;
    }

    private bool ListLevels(NetState netState, string[] args)
    {
        if (args.Length != 2)
            return false;
        
        bool shouldFilter;

        switch (args[1].ToLower())
        {
            case "true":
                shouldFilter = true;
                break;
            case "false":
                shouldFilter = false;
                break;
            default:
                return false;
        }

        Log("Levels:", netState);

        foreach (var levelValue in (Dictionary<string, int>)
                 _worldGraph.GetField<WorldGraphXML>("_levelNameToID"))
        {
            if (shouldFilter)
                if (!File.Exists(Path.Join(_config.LevelSaveDirectory, $"{levelValue.Key}.xml")))
                    continue;

            var name = _worldGraph.GetInfoLevel(levelValue.Value).InGameName;

            Log($"{levelValue.Value}: {name}", netState);
        }

        return true;
    }
}
