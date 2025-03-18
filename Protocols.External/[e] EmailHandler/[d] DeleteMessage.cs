using Server.Base.Timers.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._e__EmailHandler;

public class DeleteMessage : ExternalProtocol
{
    public override string ProtocolName => "ed";

    public ItemCatalog ItemCatalog { get; set; }
    public TimerThread TimerThread { get; set; }

    public override void Run(string[] message)
    {
        var messageId = int.Parse(message[5]);

        var items = Player.Character.EmailMessages[0].Attachments;

        var giftData = new GiftData()
        {
            MessageId = messageId,
            Items = items,
            Player = Player,
            ItemCatalog = ItemCatalog
        };

        RunGiftAnimation(giftData);
    }

    private void RunGiftAnimation(object data)
    {
        var gData = (GiftData)data;
        var player = gData.Player;

        if (player == null)
            return;

        if (player.Character == null)
            return;

        foreach (var item in gData.Items)
        {
            var getItem = ItemCatalog.GetItemFromId(item.Key);
            player.AddItem(getItem, item.Value, gData.ItemCatalog);
        }
        player.SendUpdatedInventory(false);

        player.Character.EmailMessages.Remove(0);

        player.SendXt("ed", gData.MessageId);
    }

    private class GiftData
    {
        public int MessageId { get; set; }
        public Dictionary<int, int> Items { get; set; }
        public Player Player { get; set; }
        public ItemCatalog ItemCatalog { get; set; }
    }
}
