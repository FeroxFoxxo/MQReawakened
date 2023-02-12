using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._h__HotbarHandler;

public class SetSlot : ExternalProtocol
{
    public override string ProtocolName => "hs";

    public ILogger<SetSlot> Logger { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        if (!int.TryParse(message[5], out var hotbarSlotId))
        {
            Logger.LogError("Hotbar slot ID must be an integer.");
            return;
        }

        if (!int.TryParse(message[6], out var itemId))
        {
            Logger.LogError("Item ID must be an integer.");
            return;
        }

        if (!character.TryGetItem(itemId, out var item))
        {
            Logger.LogError("Could not find item with ID {itemId} in inventory.", itemId);
            return;
        }

        character.Data.Hotbar.HotbarButtons[hotbarSlotId] = item;

        SendXt("hs", character.Data.Hotbar);
    }
}
