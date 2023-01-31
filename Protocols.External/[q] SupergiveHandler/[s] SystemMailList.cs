using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._q__SupergiveHandler;

public class SystemMailList : ExternalProtocol
{
    public override string ProtocolName => "qs";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var requestId = int.Parse(message[5]);

        var sb = new SeparatedStringBuilder('%');

        sb.Append(requestId);
        sb.Append(player.UserInfo.Mail.Count);

        foreach (var mail in player.UserInfo.Mail)
        {
            sb.Append(mail.Key);
            sb.Append(mail.Value);
        }

        SendXt("qs", sb.ToString());
    }
}
