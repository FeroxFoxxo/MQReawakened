using Server.Base.Core.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._k__SyncClock;

public class SendRoomUpdate : ExternalProtocol
{
    public override string ProtocolName => "ku";

    public override void Run(string[] message)
    {
        var ticks = message[5];
        var isSynchronized = message[6] == "0";
        var now = GetTime.GetCurrentUnixMilliseconds();

        Player.TempData.CurrentPing = now;

        if (!isSynchronized)
            SendXt("ku", ticks, now, Player.Room.TimeOffset);
        else
            SendXt("ku", ticks, now);
    }
}
