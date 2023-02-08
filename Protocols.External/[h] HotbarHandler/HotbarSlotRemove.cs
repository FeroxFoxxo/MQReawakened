using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._h__HotbarHandler;
internal class HotbarSlotRemove : ExternalProtocol
{
    public override string ProtocolName => "hr";

    public ILogger<HotbarSlotRemove> Logger { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var character = player.GetCurrentCharacter();

        if (int.TryParse(message[5], out var hotbarSlotId))
        {
            character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);

            SendXt("hr", character.Data.Hotbar);
        }
        else Logger.LogError("HotbarSlot ID must be an integer.");
    }
}
