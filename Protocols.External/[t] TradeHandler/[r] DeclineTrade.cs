using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeaderBoardTopScoresJson;

namespace Protocols.External._t__TradeHandler;
public class DeclineTrade : ExternalProtocol
{
    public override string ProtocolName => "tr";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var traderId = Player.Character.Data.TraderId;

        var originPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 1);
        var otherPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 2);

        var status = int.Parse(message[6]);

        if (!originPlayer.Character.Data.AcceptedTrade && !otherPlayer.Character.Data.AcceptedTrade)
        {
            if (traderId == 1 && !otherPlayer.Character.Data.StoppedTrade)
            {
                originPlayer.Character.Data.StoppedTrade = true;
                otherPlayer.SendXt("tc", originPlayer.Character.Data.CharacterName);
            }

            if (traderId == 2 && !originPlayer.Character.Data.StoppedTrade)
            {
                otherPlayer.Character.Data.StoppedTrade = true;
                originPlayer.SendXt("tr", otherPlayer.Character.Data.CharacterName, status);
            }
        }
    }
}

