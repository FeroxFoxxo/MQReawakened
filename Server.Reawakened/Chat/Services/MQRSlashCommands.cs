using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Server.Base.Core.Services;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;
using System.Reflection;

namespace Server.Reawakened.Chat.Services;
public class MQRSlashCommands(IServiceScopeFactory serviceFact, ReflectionUtils reflectionUtils,
    EventSink sink, ILogger<ServerConsole> logger, InternalRwConfig config) : IService
{
    private readonly Dictionary<string, SlashCommand> _commands = [];
    public List<CommandModel> ServerCommands = [];

    public void Initialize() => sink.ServerStarted += AddCommands;

    private void AddCommands(ServerStartedEventArgs e)
    {
        _commands.Clear();
        using var scope = serviceFact.CreateScope();

        foreach (var type in e.Modules.Select(m => m.GetType().Assembly.GetTypes())
                     .SelectMany(sl => sl).Where(myType => myType.IsClass && !myType.IsAbstract))
        {
            if (type.IsSubclassOf(typeof(SlashCommand)))
            {
                var createInstance =
                    reflectionUtils.CreateBuilder<SlashCommand>(type.GetTypeInfo());

                _commands.Add(createInstance(scope.ServiceProvider).CommandName, createInstance(scope.ServiceProvider));
            }
        }

        ServerCommands = [.. _commands.Select(c => c.Value as CommandModel)];
    }

    private static void Log(string message, Player player) =>
        player.Chat(CannedChatChannel.Tell, "Console", message);

    public void DisplayHelp(Player player) =>
        Log($"You can find all slash commands, run by pressing shift + enter, here: {config.GetHostAddress()}/commands", player);

    public void RunCommand(Player player, string command, string[] args)
    {
        var name = args.FirstOrDefault().ToLower();

        if (name == null || !_commands.TryGetValue(name, out var value))
        {
            logger.LogWarning("Unknown slash command: {Command}", command);
            return;
        }

        value.Run(player, args);
    }

    public Dictionary<string, SlashCommand> GetCommands() => _commands;
}
