using A2m.Server;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._c__CharacterInfoHandler;
public class ChangeAllegiance : ExternalProtocol
{
    public override string ProtocolName => "cC";

    public override void Run(string[] message)
    {
        var tribeType = int.Parse(message[5]);

        Player.Character.Write.Allegiance = (TribeType)tribeType;

        Player.SendXt("cC", tribeType, Player.Character.UserUuid);
    }
}
