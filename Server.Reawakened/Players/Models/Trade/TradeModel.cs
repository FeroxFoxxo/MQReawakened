using System.Globalization;

namespace Server.Reawakened.Players.Models.Trade;

public class TradeModel(Player tradingPlayer)
{
    public Player TradingPlayer => tradingPlayer;

    public Player InvitedPlayer { get; set; }

    public Dictionary<int, int> ItemsInTrade { get; set; } = [];
    public int BananasInTrade { get; set; } = 0;
    public bool FinalisedTrade { get; set; } = false;
    public bool AcceptedTrade { get; set; } = false;

    public static Dictionary<int, int> ReverseProposeItems(string itemData)
    {
        var items = new Dictionary<int, int>();
        var pairs = itemData.Split('|', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < pairs.Length; i += 2)
        {
            var key = int.Parse(pairs[i]);
            var value = int.Parse(pairs[i + 1]);
            items.Add(key, value);
        }

        return items;
    }

    public void ResetTrade()
    {
        ItemsInTrade.Clear();
        BananasInTrade = 0;
        FinalisedTrade = false;
    }
}
