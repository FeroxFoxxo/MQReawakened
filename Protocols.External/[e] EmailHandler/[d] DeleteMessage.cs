using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
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
                Item = item
            };

            TimerThread.DelayCall(RunGiftAnimation, giftData, TimeSpan.FromMilliseconds(3300), TimeSpan.Zero, 1);
        }
    }

    public void RunGiftAnimation(object data)
    {
        var gData = (GiftData)data;

        Player.Character.AddItem(gData.Item, gData.Item.ItemNumber);
        Player.SendUpdatedInventory(false);

        var mailMessage = Player.Character.EmailMessages[gData.MessageId];
        var mail = Player.Character.Emails[gData.MessageId];

        Player.Character.EmailMessages.Remove(mailMessage);
        Player.Character.Emails.Remove(mail);

        Player.SendXt("ed", gData.MessageId);
    }

    private class GiftData
    {
        public int MessageId;
        public ItemDescription Item;
    }
}
