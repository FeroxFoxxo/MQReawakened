using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Timers;
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

        if (messageId >= 0 && messageId <= Player.Character.EmailMessages.Count)
        {
            var item = ItemCatalog.GetItemFromId(Player.Character.EmailMessages[messageId].Item.ItemId);

            var giftData = new GiftData()
            {
                MessageId = messageId,
                Item = item,
                Player = Player,
                ItemCatalog = ItemCatalog
            };

            TimerThread.RunDelayed(RunGiftAnimation, giftData, TimeSpan.FromMilliseconds(3300));
        }
    }

    public class GiftData : PlayerTimer
    {
        public int MessageId { get; set; }
        public ItemDescription Item { get; set; }
        public ItemCatalog ItemCatalog { get; set; }
    }

    private static void RunGiftAnimation(ITimerData data)
    {
        if (data is not GiftData gift)
            return;

        gift.Player.AddItem(gift.Item, gift.Item.ItemNumber, gift.ItemCatalog);
        gift.Player.SendUpdatedInventory();

        var mailMessage = gift.Player.Character.EmailMessages[gift.MessageId];
        var mail = gift.Player.Character.Emails[gift.MessageId];

        gift.Player.Character.EmailMessages.Remove(mailMessage);
        gift.Player.Character.Emails.Remove(mail);

        gift.Player.SendXt("ed", gift.MessageId);
    }
}
