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
        var traderName = message[5];
        var tradedPlayer = PlayerHandler.GetPlayerByName(traderName);

        tradedPlayer?.SendXt("to", Player.CharacterName);
    }
}
