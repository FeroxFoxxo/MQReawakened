using Server.Base.Core.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;

namespace Protocols.External._k__SyncClock;

public class SendRoomUpdate : ExternalProtocol
{
    public override string ProtocolName => "ku";

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var ticks = message[5];
        var isSynchronized = message[6] == "0";
        var now = GetTime.GetCurrentUnixMilliseconds();

        if (isSynchronized)
            SendXt("ku", ticks, now, player.CurrentRoom.TimeOffset);
        else
            SendXt("ku", ticks, now);
    }
}
