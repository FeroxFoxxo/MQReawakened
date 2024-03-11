using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._h__HotbarHandler;

public class SwapSlot : ExternalProtocol
{
    public override string ProtocolName => "hw";

    public ILogger<SetSlot> Logger { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var hotbarSlotId = int.Parse(message[5]);
        var itemId = int.Parse(message[6]);

        if (!character.TryGetItem(itemId, out var item))
        {
            Logger.LogError("Could not find item with ID {itemId} in inventory.", itemId);
            return;
        }

        foreach (var hotbarKVP in character.Data.Hotbar.HotbarButtons)
        {
            if (hotbarKVP.Value.ItemId == itemId)
            {
                if (!character.Data.Hotbar.HotbarButtons.ContainsKey(hotbarSlotId))
                    continue;

                character.Data.Hotbar.HotbarButtons[hotbarKVP.Key] = character.Data.Hotbar.HotbarButtons[hotbarSlotId];
                character.Data.Hotbar.HotbarButtons[hotbarSlotId] = item;
            }
        }

        SendXt("hw", character.Data.Hotbar);
    }
}
