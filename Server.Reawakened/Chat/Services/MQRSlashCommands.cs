using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Events.Arguments;
using Server.Base.Core.Services;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Players;
using System.Reflection;

namespace Server.Reawakened.Chat.Services;
public class MQRSlashCommands(IServiceScopeFactory serviceFact, ReflectionUtils reflectionUtils,
    EventSink sink, ILogger<ServerConsole> logger, GetServerAddress getSA) : IService
{
    private readonly Dictionary<string, SlashCommand> _commands = [];

    public void Initialize() => sink.ServerStarted += AddCommands;

    private void AddCommands(ServerStartedEventArgs e)
    {
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
    }

    public void Log(string message, Player player) =>
        player.Chat(CannedChatChannel.Tell, "Console", message);

    public void DisplayHelp(Player player)
    {
        Log("The chat commands have been replaced with Slash Commands!", player);
        Log($"You can find these new commands here: {getSA.ServerAddress}/commands", player);
        Log("These new commands can be accessed by pressing shift + enter.", player);
    }

    public void RunCommand(Player player, string command, string[] args)
    {
        var name = args.FirstOrDefault();

        if (name == null || !_commands.TryGetValue(name, out var value))
        {
            logger.LogWarning($"Unknown slash command: {command}");
            return;
        }

        value.Run(player, args);
    }

    public Dictionary<string, SlashCommand> GetCommands() => _commands;
}
