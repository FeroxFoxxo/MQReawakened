using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Models;
using Server.Base.Accounts.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Services;
using System.Xml;

namespace Protocols.System._xml__System;

public class Login : SystemProtocol
{
    public override string ProtocolName => "login";

    public AccountHandler AccountHandler { get; set; }
    public UserInfoHandler UserInfoHandler { get; set; }
    public DatabaseContainer DatabaseContainer { get; set; }
    public ILogger<Login> Logger { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var username = xmlDoc.SelectSingleNode("/msg/body/login/nick")?.FirstChild?.Value;
        var password = xmlDoc.SelectSingleNode("/msg/body/login/pword")?.FirstChild?.Value;

        var reason = AccountHandler.GetAccount(username, password, NetState);

        if (reason == AlrReason.Accepted)
        {
            lock (DatabaseContainer.Lock)
            {
                var account = NetState.Get<Account>();

                foreach (var player in DatabaseContainer.GetPlayersByUserId(account.Id))
                    player.Remove(Logger);

                if (!DatabaseContainer.AnyPlayersByUserId(account.Id))
                {
                    UserInfoHandler.InitializeUser(NetState);
                    SendXml(
                        "logOK",
                        $"<login id='{account.Id}' mod='{account.IsModerator()}' n='{username}' />"
                    );
                    return;
                }
            }

            reason = AlrReason.PlayerLoggedIn;
        }

        SendXml("logKO", $"<login e='{reason.GetErrorValue()}' />");
    }
}
