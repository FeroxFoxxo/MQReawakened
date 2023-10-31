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
        var traderId = Player.Character.Data.TraderId;

        var originPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 1);
        var otherPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 2);

        originPlayer.Character.Data.TradeDeal = false;
        otherPlayer.Character.Data.TradeDeal = false;

        originPlayer.Character.ItemsInTrade.Clear();
        otherPlayer.Character.ItemsInTrade.Clear();

        if (traderId == 1)
        {
            originPlayer.Character.ItemsInTrade.Clear();
            otherPlayer.SendXt("te", originPlayer.Character.Data.CharacterName);
        }

        if (traderId == 2)
        {
            otherPlayer.Character.ItemsInTrade.Clear();
            originPlayer.SendXt("te", otherPlayer.Character.Data.CharacterName);
        }
    }
}
