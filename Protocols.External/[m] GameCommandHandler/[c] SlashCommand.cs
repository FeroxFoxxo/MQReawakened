using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Services;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._m__GameCommandHandler;

public class SlashCommand : ExternalProtocol
{
    public override string ProtocolName => "mc";

    public MQRSlashCommands SlashCommands { get; set; }

    public override void Run(string[] message)
    {
        var command = message[5];
        var args = command.Split(' ').Select(s => s.Trim()).ToArray();

        if (Player.Account.AccessLevel < AccessLevel.Moderator)
            return;

        SlashCommands.RunCommand(Player, command, args);
    }
}
