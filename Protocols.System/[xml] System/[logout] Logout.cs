using Server.Reawakened.Network.Protocols;
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
        Player?.QuickJoinRoom(-1, WorldHandler);

        SendXml("logout", string.Empty);
    }
}
