using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._c__CharacterInfoHandler;

public class RefreshBalance : ExternalProtocol
{
    public override string ProtocolName => "cg";

    public override void Run(string[] message) => Player.SendCashUpdate();
}
