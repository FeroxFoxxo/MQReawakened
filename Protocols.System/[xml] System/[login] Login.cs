using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Models;
using Server.Base.Accounts.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Services;
using System.Xml;

namespace Protocols.System._xml__System;

public class Login : SystemProtocol
{
    public override string ProtocolName => "login";

    public AccountHandler AccountHandler { get; set; }
    public UserInfoHandler UserInfoHandler { get; set; }
    public PlayerHandler PlayerHandler { get; set; }
    public ILogger<Login> Logger { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var username = xmlDoc.SelectSingleNode("/msg/body/login/nick")?.FirstChild?.Value;
        var password = xmlDoc.SelectSingleNode("/msg/body/login/pword")?.FirstChild?.Value;

        var reason = AccountHandler.GetAccount(username, password, NetState);

        if (reason == AlrReason.Accepted)
        {
            var account = NetState.Get<Account>();

            if (!PlayerHandler.PlayerList.Any(p => p.UserId == account.UserId))
            {
                UserInfoHandler.InitializeUser(NetState);
                SendXml(
                    "logOK",
                    $"<login id='{NetState.Get<Account>().UserId}' mod='{NetState.Get<Account>().IsModerator()}' n='{username}' />"
                );
                return;
            }

            reason = AlrReason.PlayerLoggedIn;
        }

        SendXml("logKO", $"<login e='{reason.GetErrorValue()}' />");
    }
}
