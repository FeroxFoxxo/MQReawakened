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
        var tradedPlayer = PlayerHandler.PlayerList
            .FirstOrDefault(p => p.Character.Data.CharacterName == message[5]);

        var status = (DeclineType) int.Parse(message[6]);

        if (status == DeclineType.InviteeRejection)
            tradedPlayer?.SendXt("tc", Player.Character.Data.CharacterName);
        else if (status is DeclineType.PlayerDnD or DeclineType.PlayerBusy)
            tradedPlayer?.SendXt("tr", Player.Character.Data.CharacterName, status);
        else
            Logger.LogError("Unknown decline type: {DeclineType}", status);
    }
}

