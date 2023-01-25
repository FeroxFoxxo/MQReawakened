using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Modals;
using Server.Reawakened.Core.Network.Protocols;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using System.Xml;

namespace Protocols.System._xml__System;

public class AutoJoin : SystemProtocol
{
    public override string ProtocolName => "autoJoin";

    public LevelHandler LevelHandler { get; set; }
    public ILogger<AutoJoin> Logger { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var user = NetState.Get<Player>();
        var account = NetState.Get<Account>();

        Level newLevel;

        try
        {
            newLevel = LevelHandler.GetLevelFromId(0);
        }
        catch (NullReferenceException)
        {
            newLevel = null;
        }

        if (newLevel == null)
        {
            Logger.LogError("Could not find level! Returning...");
            return;
        }

        user.JoinLevel(NetState, newLevel);
        SendXt("cx", user.UserInfo.GetPropertyValues(account));
        SendXt("cl", $"{user.UserInfo.LastCharacterSelected}{(user.UserInfo.Characters.Count > 0 ? "%" : "")}" +
                     string.Join('%', user.UserInfo.Characters.Select(c => c.ToString()))
        );
    }
}
