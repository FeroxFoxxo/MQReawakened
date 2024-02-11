using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._e__EmailHandler;

public class DeleteMessage : ExternalProtocol
{
    public override string ProtocolName => "ed";

    public ItemCatalog ItemCatalog { get; set; }
    public TimerThread TimerThread { get; set; }

    public override void Run(string[] message)
    {
        var messageId = int.Parse(message[5]);

        if (messageId >= 0 && messageId <= Player.Character.EmailMessages.Count)
        {
            var item = ItemCatalog.GetItemFromId(Player.Character.EmailMessages[messageId].Item.ItemId);

            var giftData = new GiftData()
            {
                MessageId = messageId,
                Item = item,
                Player = Player
            };

            TimerThread.DelayCall(RunGiftAnimation, giftData, TimeSpan.FromMilliseconds(3300), TimeSpan.Zero, 1);
        }
    }

    public static void RunGiftAnimation(object data)
    {
        var gData = (GiftData)data;
        var player = gData.Player;

        if (player == null)
            return;

        if (player.Character == null)
            return;

        player.AddItem(gData.Item, gData.Item.ItemNumber);
        player.SendUpdatedInventory(false);

        var mailMessage = player.Character.EmailMessages[gData.MessageId];
        var mail = player.Character.Emails[gData.MessageId];

        player.Character.EmailMessages.Remove(mailMessage);
        player.Character.Emails.Remove(mail);

        player.SendXt("ed", gData.MessageId);
    }

    private class GiftData
    {
        public int MessageId { get; set; }
        public ItemDescription Item { get; set; }
        public Player Player { get; set; }
    }
}
