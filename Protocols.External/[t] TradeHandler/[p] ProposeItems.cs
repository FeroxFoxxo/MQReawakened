using A2m.Server;
using Server.Base.Accounts.Models;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Protocols.External._t__TradeHandler;
public class ProposeItems : ExternalProtocol
{
    public override string ProtocolName => "tp";

    public PlayerHandler PlayerHandler { get; set; }

    public override void Run(string[] message)
    {
        var itemsString = message[5];
        var matches = Regex.Matches(itemsString, @"(\d+\|\d+)");
        var items = matches.Cast<Match>().Select(match => match.Value).ToArray();
        var bananas = int.Parse(message[6]);

        var traderId = Player.Character.Data.TraderId;

        var originPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 1);
        var otherPlayer = PlayerHandler.PlayerList.Find(p => p.Character.Data.TraderId == 2);

        if (traderId == 1)
        {
            foreach (var item in items)
            {
                var args = item.Split('|');
                var itemId = int.Parse(args[0]);
                var amount = int.Parse(args[1]);
                originPlayer.Character.ItemsInTrade.Add(itemId, amount);
            }

            originPlayer.Character.Data.BananasInTrade = bananas;
            foreach (var item in originPlayer.Character.ItemsInTrade)
                otherPlayer.SendXt("tp", GenerateItemString(items), bananas);
        }

        if (traderId == 2)
        {
            foreach (var item in items)
            {
                var args = item.Split('|');
                var itemId = int.Parse(args[0]);
                var amount = int.Parse(args[1]);
                otherPlayer.Character.ItemsInTrade.Add(itemId, amount);
            }

            otherPlayer.Character.Data.BananasInTrade = bananas;
            foreach (var item in otherPlayer.Character.ItemsInTrade)
                originPlayer.SendXt("tp", GenerateItemString(items), bananas);
        }
    }

    public string GenerateItemString(string[] itemData)
    {
        var sb = new SeparatedStringBuilder('|');

        for (var i = 0; i < itemData.Length; i++)
            sb.Append(itemData[i]);

        return sb.ToString();
    }
}
