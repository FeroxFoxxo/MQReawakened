using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Models.Planes;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;

namespace Protocols.External._h__HotbarHandler;

internal class HotbarSlotSet : ExternalProtocol
{
    public override string ProtocolName => "hs";

    public ILogger<HotbarSlotSet> Logger { get; set; }

    public override void Run(string[] message)
    {
        for (int i = 0; i < message.Length; i++)
        {
            Logger.LogInformation("\t{i} {msg}", i, message[i]);
        }

        var player = NetState.Get<Player>();

        var character = player.GetCurrentCharacter();

        if (int.TryParse(message[5], out var hotbarSlotId))
        {
            if (int.TryParse(message[6], out var itemId))
            {
                if (character.TryGetItem(itemId, out var item))
                {
                    character.Data.Hotbar.HotbarButtons[hotbarSlotId] = item;

                    SendXt("hs", character.Data.Hotbar);
                }
                else Logger.LogError("Could not find Item with ID {itemId} in inventory.", itemId);
            }
            else Logger.LogError("Item ID must be an integer.");
        }
        else Logger.LogError("HotbarSlot ID must be an integer.");
    }
}
