using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._h__HotbarHandler;

public class RemoveSlot : ExternalProtocol
{
    public override string ProtocolName => "hr";

    public ILogger<RemoveSlot> Logger { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var hotbarSlotId = int.Parse(message[5]);

        character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);

        SendXt("hr", character.Data.Hotbar);
    }
}
