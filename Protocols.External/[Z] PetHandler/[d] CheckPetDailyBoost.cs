using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._Z__PetHandler;
public class CheckPetDailyBoost : ExternalProtocol
{
    public override string ProtocolName => "Zd";

    public override void Run(string[] message) => Player.SendXt("Zd", 0);
}
