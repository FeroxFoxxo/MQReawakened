using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._h__HotbarHandler;

public class RemoveSlot : ExternalProtocol
{
    public override string ProtocolName => "hr";

    public ILogger<RemoveSlot> Logger { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var character = player.GetCurrentCharacter();

        if (!int.TryParse(message[5], out var hotbarSlotId))
        {
            Logger.LogError("Hotbar slot ID must be an integer.");
            return;
        }

        character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);

        SendXt("hr", character.Data.Hotbar);
    }
}
