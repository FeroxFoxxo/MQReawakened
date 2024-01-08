using A2m.Server.MessageCenter.Proto;
using A2m.Server;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._e__EmailHandler;

public class DeleteMessage : ExternalProtocol
{
    public override string ProtocolName => "ed";

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var messageId = int.Parse(message[5]);

        if (messageId >= 0 && messageId <= Player.Character.EmailMessages.Count)
        {
            var item = ItemCatalog.GetItemFromId(Player.Character.EmailMessages[messageId].Item.ItemId);

            _ = RunGiftAnimation(item, messageId);
        }
    }

    public async Task RunGiftAnimation(ItemDescription item, int messageId)
    {
        await Task.Delay(3300); //Adds item after open gift animation.

        Player.Character.AddItem(item, item.ItemNumber);
        Player.SendUpdatedInventory(false);

        var mailMessage = Player.Character.EmailMessages[messageId];
        var mail = Player.Character.Emails[messageId];

        Player.Character.EmailMessages.Remove(mailMessage);
        Player.Character.Emails.Remove(mail);

        Player.SendXt("ed", messageId);
    }
}
