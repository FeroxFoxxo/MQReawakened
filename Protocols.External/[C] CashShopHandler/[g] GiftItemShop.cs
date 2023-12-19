﻿using A2m.Server;
using A2m.Server.MessageCenter.Proto;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.System;
using Server.Reawakened.XMLs.Bundles;
using System.Security.Cryptography.X509Certificates;
using static VendorCatalogsXML;

namespace Protocols.External._C__CashShopHandler;

public class GiftItemShop : ExternalProtocol
{
    public override string ProtocolName => "Cg";

    public ItemCatalog ItemCatalog { get; set; }
    public PlayerHandler PlayerHandler { get; set; }
    public ILogger<GiftItemShop> Logger { get; set; }

    public override void Run(string[] message)
    {
        var cashShop = (Cashshop)int.Parse(message[5]);

        if (cashShop != Cashshop.CashShop)
        {
            Logger.LogWarning("Unknown cashshop of type {Type}!", cashShop);
            return;
        }

        var friendName = message[6];
        var messageDesc = message[7];
        var itemId = int.Parse(message[8]);
        var backgroundId = int.Parse(message[9]);
        var packageId = int.Parse(message[10]);

        var friend = PlayerHandler.GetPlayerByName(friendName);

        var package = ItemCatalog.GetItemFromId(packageId);
        Player.RemoveBananas(package.RegularPrice);

        var item = ItemCatalog.GetItemFromId(itemId);
        Player.RemoveNCash(item.RegularPrice);

        var mailId = 0;
        foreach (var mailItem in friend.Character.Emails)
            mailId++;

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
        friend.Character.Emails.Add(emailHeader);

        friend.SendXt("en", emailHeader.ToString());

        var attachments = new Dictionary<ItemDescription, int> { { item, 1 } }; //I don't think you can gift more than 1 item at a time,
                                                                                //so I'm not entirely sure how attachments is supposed to be used.                                                                        
        var emailMessage = new EmailMessageModel()
        {
            EmailHeaderModel = emailHeader,
            Body = messageDesc,
            BackgroundId = backgroundId,
            PackageId = packageId,
            Item = item,
            Attachments = attachments
        };
        friend.Character.EmailMessages.Add(emailMessage);

        var mail = friend.Character.Emails;
        friend.SendXt("ei", mail.ToString());

        Player.SendXt("es", friend.CharacterName, 1);
    }
}



