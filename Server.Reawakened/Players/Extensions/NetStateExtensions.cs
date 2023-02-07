using Server.Base.Network;
using Server.Reawakened.Network.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class NetStateExtensions
{
    public static void SendSyncEventToPlayer(this NetState state, SyncEvent syncEvent)
    {
        var syncEventMsg = syncEvent.EncodeData();
        state.SendXt("ss", syncEventMsg);
    }
}
