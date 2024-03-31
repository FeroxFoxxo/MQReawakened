using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._h__HotbarHandler;

public class RemoveSlot : ExternalProtocol
{
    public override string ProtocolName => "hr";

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var hotbarSlotId = int.Parse(message[5]);

        Player.RemoveHotbarSlot(hotbarSlotId, ItemCatalog);

        SendXt("hr", character.Data.Hotbar);
    }
}
