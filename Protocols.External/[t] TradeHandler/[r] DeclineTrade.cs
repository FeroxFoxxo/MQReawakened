using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Trade;

namespace Protocols.External._t__TradeHandler;

public class DeclineTrade : ExternalProtocol
{
    public override string ProtocolName => "tr";

    public ILogger<DeclineType> Logger { get; set; }

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var traderName = message[5];
        var tradedPlayer = PlayerHandler.GetPlayerByName(traderName);

        var status = (DeclineType) int.Parse(message[6]);

        if (status == DeclineType.InviteeRejection)
            tradedPlayer?.SendXt("tc", Player.CharacterName);
        else if (status is DeclineType.PlayerBusy or DeclineType.PlayerDnD)
            tradedPlayer?.SendXt("tr", Player.CharacterName, (int) status);
        else
            Logger.LogError("Unknown decline type: {DeclineType}", status);
    }
}

