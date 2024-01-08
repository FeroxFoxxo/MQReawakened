﻿using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._M__MinigameHandler;
public class BestResultForLevel : ExternalProtocol
{
    public override string ProtocolName => "MB";

    public override void Run(string[] message)
    {
        var levelName = message[5];

        if (Player.TempData.ArenaModel.BestTimeForLevel.ContainsKey(levelName))
            Player.SendXt("MB", Player.TempData.ArenaModel.BestTimeForLevel[levelName]); 
    }
}