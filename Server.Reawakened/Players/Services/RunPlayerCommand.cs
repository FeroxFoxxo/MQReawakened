using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Database.Accounts;
using Server.Base.Network.Enums;
using Server.Base.Network.Services;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Players.Extensions;
using Server.Base.Core.Extensions;

namespace Server.Reawakened.Players.Services;

public class RunPlayerCommand(ServerConsole console, EventSink sink,
    ILogger<RunPlayerCommand> logger, UserInfoHandler userInfoHandler,
    AccountHandler accountHandler, CharacterHandler characterHandler,
    NetStateHandler handler, MQRSlashCommands slashCommands) : IService
{
    public void Initialize() => sink.WorldLoad += Load;

    public void Load() =>
        console.AddCommand(
            "runCommand",
            "Runs a command for a given player.",
            NetworkType.Server,
            _ => RunPlayerCmd()
        );

    private void RunPlayerCmd()
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

        if (EnvironmentExt.IsContainerOrNonInteractive())
        {
            logger.LogWarning("Non-interactive mode; skipping manual player command input.");
            return;
        }

        var command = ConsoleExt.ReadLineOrDefault(logger, null)?.Trim();

        slashCommands.RunCommand(player, command.Split(' ')[0], command.Split(' '));
    }
}
