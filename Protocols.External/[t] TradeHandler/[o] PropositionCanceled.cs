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
        var traderId = Player.Character.Data.TraderId;

        var originPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 1);
        var otherPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 2);


        originPlayer.Character.Data.TradeDeal = false;
        otherPlayer.Character.Data.TradeDeal = false;

        if (traderId == 1)
        {
            originPlayer.Character.ItemsInTrade.Clear();
            otherPlayer.SendXt("to", originPlayer.Character.Data.CharacterName);
        }

        if (traderId == 2)
        {
            otherPlayer.Character.ItemsInTrade.Clear();
            originPlayer.SendXt("to", otherPlayer.Character.Data.CharacterName);
        }
    }
}
