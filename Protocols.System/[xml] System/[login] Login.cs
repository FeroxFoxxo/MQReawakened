using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Database.Accounts;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Base;
using System.Xml;

namespace Protocols.System._xml__System;

public class Login : SystemProtocol
{
    public override string ProtocolName => "login";

    public AccountHandler AccountHandler { get; set; }
    public UserInfoHandler UserInfoHandler { get; set; }
    public PlayerContainer PlayerContainer { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ILogger<Login> Logger { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var username = xmlDoc.SelectSingleNode("/msg/body/login/nick")?.FirstChild?.Value;
        var password = xmlDoc.SelectSingleNode("/msg/body/login/pword")?.FirstChild?.Value;

        var reason = AccountHandler.GetAccount(username, password, NetState);

        if (reason == AlrReason.Accepted)
        {
            lock (PlayerContainer.Lock)
            {
                var account = NetState.Get<AccountModel>();

                foreach (var player in PlayerContainer.GetPlayersByUserId(account.Id))
                    player.Remove(Logger);

                if (!PlayerContainer.AnyPlayersByUserId(account.Id))
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
