﻿using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._l__ExtLevelEditor;

public class StartPlay : ExternalProtocol
{
    public override string ProtocolName => "ly";

    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message) => Player.SendLevelChange(WorldHandler);
}
