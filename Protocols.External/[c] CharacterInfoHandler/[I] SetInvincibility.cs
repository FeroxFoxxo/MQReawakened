using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._c__CharacterInfoHandler;

public class SetInvincibility : ExternalProtocol
{
    public override string ProtocolName => "cI";

    public override void Run(string[] message)
    {
        // This protocol is only used for levelup invincibility
        var invincibilityStatus = int.Parse(message[5]) == 1;
        
        if (invincibilityStatus)
            Player.TemporaryInvincibility(1);
        
        SendXt("cI", invincibilityStatus ? 1 : 0);
    }
}
