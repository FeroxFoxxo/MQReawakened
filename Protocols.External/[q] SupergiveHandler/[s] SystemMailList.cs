using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.System;

namespace Protocols.External._q__SupergiveHandler;

public class SystemMailList : ExternalProtocol
{
    public override string ProtocolName => "qs";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var requestId = int.Parse(message[5]);
        var mail = GetSystemMail(player.UserInfo.Mail);

        SendXt("qs", requestId, player.UserInfo.Mail.Count, mail);
    }

    public static string GetSystemMail(Dictionary<int, SystemMailModel> mailList)
    {
        var sb = new SeparatedStringBuilder('%');

        foreach (var mail in mailList)
        {
            sb.Append(mail.Key);
            sb.Append(mail.Value);
        }

        return sb.ToString();
    }
}
