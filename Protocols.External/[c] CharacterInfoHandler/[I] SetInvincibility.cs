using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._c__CharacterInfoHandler;

public class SetInvincibility : ExternalProtocol
{
    public override string ProtocolName => "cI";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var invincibilityStatus = int.Parse(message[5]) == 1;
        player.Invincible = invincibilityStatus;
        SendXt("cI", invincibilityStatus ? 1 : 0);
    }
}
