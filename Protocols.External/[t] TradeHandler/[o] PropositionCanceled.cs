using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._t__TradeHandler;

public class PropositionCanceled : ExternalProtocol
{
    public override string ProtocolName => "to";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var tradedPlayer = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.CharacterName == message[5]);

        tradedPlayer?.SendXt("to", Player.CharacterName);
    }
}
