using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._a__ChatHandler;

public class FreeChat : ExternalProtocol
{
    public override string ProtocolName => "ae";

    public ILogger<FreeChat> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        var channelType = (CannedChatChannel)Convert.ToInt32(message[5]);
        var chatMessage = message[6];
        var recipientName = message[7];

        if (chatMessage.StartsWith("."))
        {
            var args = chatMessage.Contains(' ') ? chatMessage[1..].Split(' ') : new[] { chatMessage[1..] };

            switch (args[0])
            {
                case "item":
                    {
                        if (args.Length is < 2 or > 3)
                        {
                            Logger.LogDebug("Usage: .item [itemId] [amount]");
                            break;
                        }

                        var itemId = Convert.ToInt32(args[1]);
                        var item = ItemCatalog.GetItemFromId(itemId);

                        if (item == null)
                        {
                            Logger.LogDebug("Can't find item with id {ItemId}", itemId);
                            return;
                        }

                        var amount = 1;

                        if (args.Length == 3)
                        {
                            amount = Convert.ToInt32(args[2]);

                            if (amount <= 0)
                                amount = 1;
                        }

                        character.AddItem(item, amount);

                        NetState.SendUpdatedInventory();

                        Logger.LogInformation(
                            "{CharacterId} received {ItemName} x{Amount}",
                            character.Data.CharacterName, item.ItemName, amount
                        );
                    }
                    break;
            }
        }
        else if (channelType == CannedChatChannel.Speak)
        {
            player.CurrentLevel.Chat(channelType, character.Data.CharacterName, chatMessage);
        }
        else
        {
            Logger.LogError("No chat handler found for {ChannelType} to '{Recipient}' for '{Message}'",
                channelType, recipientName, chatMessage);
        }
    }
}
