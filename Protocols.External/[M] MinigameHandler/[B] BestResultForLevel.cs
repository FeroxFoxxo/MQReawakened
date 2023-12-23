using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._M__MinigameHandler;
public class BestResultForLevel : ExternalProtocol
{
    public override string ProtocolName => "MB";

    public override void Run(string[] message)
    {
        foreach (var msg in message)
            Console.WriteLine(msg);

        var levelName = message[5];

            Player.SendXt("MB", Player.Room.Time);
    }
}
