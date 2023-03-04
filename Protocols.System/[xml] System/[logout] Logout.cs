using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using System.Xml;

namespace Protocols.System._xml__System;

public class Logout : SystemProtocol
{
    public override string ProtocolName => "logout";

    public WorldHandler WorldHandler { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var player = NetState.Get<Player>();

        player?.QuickJoinRoom(-1, WorldHandler);

        SendXml("logout", string.Empty);
    }
}
