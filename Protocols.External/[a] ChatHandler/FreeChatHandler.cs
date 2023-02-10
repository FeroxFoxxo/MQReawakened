using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Entities;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols.External._a__ChatHandler;

internal class FreeChatHandler : ExternalProtocol
{
    public override string ProtocolName => "ae";

    public ILogger<FreeChatHandler> Logger { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        var channelId = (CannedChatChannel)Convert.ToInt32(message[5]);
        if (message[6][0] == '.')
        {
            var args = message[6].Contains(' ') ? message[6][1..].Split(' ') : (new string[] { message[6][1..] });
            
            switch (args[0])
            {
                case "item": // .item id [amound]
                    {
                        switch(args.Length)
                        {
                            case 2:
                                {
                                    var itemId = Convert.ToInt32(args[1]);
                                    var item = ItemCatalog.GetItemFromId(itemId);
                                    if (item == null)
                                    {
                                        Logger.LogDebug($"Can't find item with id {itemId}");
                                        return;
                                    }

                                    character.AddItem(item, 1);
                                    NetState.SendUpdatedInventory();
                                    Logger.LogInformation($"{character.Data.CharacterName} received {item.ItemName} x1");
                                }
                                break;

                            case 3:
                                {
                                    var itemId = Convert.ToInt32(args[1]);
                                    var item = ItemCatalog.GetItemFromId(itemId);
                                    if (item == null)
                                    {
                                        Logger.LogDebug($"Can't find item with id {itemId}");
                                        return;
                                    }
                                    var amount = Convert.ToInt32(args[2]);
                                    if (amount <= 0)
                                        amount = 1;

                                    character.AddItem(item, amount);
                                    NetState.SendUpdatedInventory();
                                    Logger.LogInformation($"{character.Data.CharacterName} received {item.ItemName} x{amount}");
                                }
                                break;

                            default:
                                Logger.LogDebug("Usage: .item itemId [amount]");
                                break;
                        }
                    }
                    break;
            }
        }
        else if (channelId == CannedChatChannel.Speak)
        {
            player.CurrentLevel.Chat(channelId, character.Data.CharacterName, message[6]);
        }
    }
}
