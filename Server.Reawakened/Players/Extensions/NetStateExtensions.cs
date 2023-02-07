using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Network.Extensions;

namespace Server.Reawakened.Players.Extensions;

public static class NetStateExtensions
{
    public static void SendSyncEventToPlayer(this NetState state, SyncEvent syncEvent)
    {
        var syncEventMsg = syncEvent.EncodeData();
        state.SendXt("ss", syncEventMsg);
    }

    public static bool IsPlayerOnline(this NetStateHandler handler, int userId, out NetState netState, out Player player)
    {
        netState = handler.FindUser(userId);
        player = null;

        if (netState == null)
            return false;

        player = netState.Get<Player>();

        return player != null;
    }
}
