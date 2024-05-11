using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._l__ExtLevelEditor;

public class FastTravel : ExternalProtocol
{
    public override string ProtocolName => "lF";

    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        var levelId = int.Parse(message[5]);
        var goId = int.Parse(message[6]);

        WorldHandler.UsePortal(Player, levelId, goId);
    }
}
