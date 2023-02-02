using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using System.Xml;

namespace Protocols.System._xml__System;

public class Logout : SystemProtocol
{
    public override string ProtocolName => "logout";

    public LevelHandler LevelHandler { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var player = NetState.Get<Player>();

        player?.QuickJoinLevel(-1, NetState, LevelHandler);

        SendXml("logout", string.Empty);
    }
}
