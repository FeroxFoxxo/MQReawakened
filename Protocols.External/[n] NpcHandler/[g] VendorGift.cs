using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.System;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._n__NpcHandler;

public class VendorGift : ExternalProtocol
{
    public override string ProtocolName => "ng";

    public ItemCatalog ItemCatalog { get; set; }
    public PlayerContainer PlayerContainer { get; set; }
    public CharacterHandler CharacterHandler { get; set; }
    public ILogger<VendorGift> Logger { get; set; }
    public ServerRConfig Config { get; set; }

    public override void Run(string[] message)
    {
        if (!Player.Room.ContainsEntity(message[5]))
        {
            Logger.LogWarning("Unknown vendor with id {Type}!", message[5]);
            return;
        }

        var isOnline = true;
        var friendName = message[6];
        var messageDesc = message[7];
        var itemId = int.Parse(message[8]);
        var backgroundId = int.Parse(message[9]);
        var packageId = int.Parse(message[10]);

        var friend = PlayerContainer.GetPlayerByName(friendName);
        CharacterDbEntry friendImage = null;

        //If friend is null, they're offline. Try a different way
        if (friend is null)
        {
            var friendEntity = CharacterHandler.GetCharacterFromName(friendName);
            friendImage = CharacterHandler.Get(friendEntity.Id);
            isOnline = false;
        }

        var package = ItemCatalog.GetItemFromId(packageId);
        var item = ItemCatalog.GetItemFromId(itemId);

        if (item.Currency == CurrencyType.NickCash)
        {
            Player.RemoveBananas(package.RegularPrice);
            Player.RemoveNCash(item.RegularPrice);
        }
        else
            Player.RemoveBananas(package.RegularPrice + item.RegularPrice);

        var mailId = isOnline ? friend.Character.EmailMessages.Count : friendImage.EmailMessages.Count;
        while (true)
        {
            if (isOnline && friend.Character.EmailMessages.ContainsKey(mailId) || !isOnline && friendImage != null && friendImage.EmailMessages.ContainsKey(mailId))
                mailId++;
            else
                break;
        }

        var emailHeader = new EmailHeaderModel()
        {
            MessageId = mailId,
            From = Player.CharacterName,
            To = friendName,
            CategoryId = EmailCategory.Gift,
            Subject = messageDesc,
            SentTime = Player.Room.Time.ToString(),
            Status = EmailHeader.EmailStatus.UnreadMail
        };

        var attachments = new Dictionary<int, int> { { item.ItemId, 1 } };
        var emailMessage = new EmailMessageModel()
        {
            EmailHeaderModel = emailHeader,
            Body = messageDesc,
            BackgroundId = backgroundId,
            PackageId = packageId,
            Item = item.ItemId,
            Attachments = attachments
        };

        if (isOnline)
            friend.Character.EmailMessages.Add(mailId, emailMessage);
        else
        {
            friendImage.EmailMessages.Add(mailId, emailMessage);
            CharacterHandler.Update(friendImage);
        }

        if (isOnline)
        {
            var sb = new SeparatedStringBuilder('&');
            foreach (var email in friend.Character.EmailMessages)
                sb.Append(email.Value.EmailHeaderModel.ToString());

            friend.SendXt("en", emailHeader.ToString());
            friend.SendXt("ei", sb.ToString());
        }
    }
}
