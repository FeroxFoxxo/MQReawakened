using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._n__NpcHandler;

public class BuyItems : ExternalProtocol
{
    public override string ProtocolName => "nb";

    public ItemCatalog ItemCatalog { get; set; }

    public QuestCatalog QuestCatalog { get; set; }

    public ILogger<BuyItems> Logger { get; set; }

    public override void Run(string[] message)
    {
        foreach (var item in message[6].Split('|'))
            if (!string.IsNullOrEmpty(item))
                AddItemToPlayer(item.Split(":"), int.Parse(message[7]));

        Player.SendUpdatedInventory(false);
    }

    private void AddItemToPlayer(string[] args, int vendorGoId)
    {
        var itemId = int.Parse(args[0]);
        var amountOfItems = int.Parse(args[1]);
        var itemDetails = ItemCatalog.GetItemFromId(itemId);

        if (HandlePlayerCurrency(itemDetails.Currency, itemDetails.RegularPrice * amountOfItems))
            Player.Character.AddItem(itemDetails, amountOfItems);

        Player.CheckObjective(QuestCatalog, ObjectiveEnum.Buyitem, vendorGoId, itemId, amountOfItems);
    }

    private bool HandlePlayerCurrency(CurrencyType currencyType, int totalCost)
    {
        switch (currencyType)
        {
            case CurrencyType.Banana:
                Player.RemoveBananas(totalCost);
                return true;
            case CurrencyType.NickCash:
                Player.RemoveNCash(totalCost);
                return true;
            default:
                Logger.LogError("Currency type invalid! ({CurrencyType})", currencyType);
                return false;
        }
    }
}
