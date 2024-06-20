using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._h__HotbarHandler;

public class SwapSlot : ExternalProtocol
{
    public override string ProtocolName => "hw";

    public ILogger<SetSlot> Logger { get; set; }
    public ItemRConfig ItemRConfig { get; set; }

    public override void Run(string[] message)
    {
        var hotbarSlotId = int.Parse(message[5]);
        var itemId = int.Parse(message[6]);

        if (!Player.Character.TryGetItem(itemId, out var item))
        {
            Logger.LogError("Could not find item with ID {itemId} in inventory.", itemId);
            return;
        }

        foreach (var hotbarKVP in Player.Character.Hotbar.HotbarButtons)
        {
            if (hotbarKVP.Value.ItemId == itemId)
            {
                if (!Player.Character.Hotbar.HotbarButtons.ContainsKey(hotbarSlotId))
                    continue;

                var otherItem = Player.Character.Hotbar.HotbarButtons[hotbarSlotId];

                Player.SetHotbarSlot(hotbarKVP.Key, otherItem, ItemRConfig);
                Player.SetHotbarSlot(hotbarSlotId, item, ItemRConfig);
            }
        }

        SendXt("hw", Player.Character.Hotbar);
    }
}
