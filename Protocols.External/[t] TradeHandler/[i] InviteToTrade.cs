﻿using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Trade;

namespace Protocols.External._t__TradeHandler;

public class InviteToTrade : ExternalProtocol
{
    public override string ProtocolName => "ti";

    public PlayerContainer PlayerContainer { get; set; }
    public ServerRConfig Config { get; set; }

    public override void Run(string[] message)
    {
        if (Player.TempData.TradeModel != null)
            return;

        if (!Config.Trading)
        {
            Player.SendWarningMessage("trading");
            return;
        }

        var traderName = message[5];
        var invitedPlayer = PlayerContainer.GetPlayerByName(traderName);

        if (invitedPlayer == null)
            return;

        Player.TempData.TradeModel = new TradeModel(invitedPlayer);
        invitedPlayer.TempData.TradeModel = new TradeModel(Player);

        Player.TempData.TradeModel.InvitedPlayer = invitedPlayer;

        if (!invitedPlayer.Character.Blocked.Contains(Player.CharacterId))
            invitedPlayer?.SendXt("ti", Player.CharacterName);
    }
}
