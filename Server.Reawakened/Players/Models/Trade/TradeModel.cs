using System.Globalization;

namespace Server.Reawakened.Players.Models.Temporary;

public class TradeModel(Player tradingPlayer)
{
    public Player TradingPlayer = tradingPlayer;

    public Dictionary<int, int> ItemsInTrade = [];
    public int BananasInTrade;

    public static Dictionary<int, int> ReverseProposeItems(string itemData)
    {
        var items = new Dictionary<int, int>();
        var pairs = itemData.Split('|', StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < pairs.Length; i += 2)
        {
            var key = int.Parse(pairs[i], CultureInfo.InvariantCulture);
            var value = int.Parse(pairs[i + 1], CultureInfo.InvariantCulture);
            items.Add(key, value);
        }

        return items;
    }
}
