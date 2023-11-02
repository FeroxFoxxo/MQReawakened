using Server.Reawakened.Network.Protocols;

namespace Protocols.External._c__CharacterInfoHandler;

public class SetInvincibility : ExternalProtocol
{
    public override string ProtocolName => "cI";

    public override void Run(string[] message)
    {
        var invincibilityStatus = int.Parse(message[5]) == 1;
        Player.TempData.Invincible = invincibilityStatus;
        SendXt("cI", invincibilityStatus ? 1 : 0);
    }
}
