using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._t__TradeHandler;

public class PropositionRejected : ExternalProtocol
{
    public override string ProtocolName => "te";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var tradingPlayer = Player.TempData.TradeModel?.TradingPlayer;

        tradingPlayer?.SendXt("te", Player.Character.Data.CharacterName);
    }
}
