using Server.Base.Network;
using Server.Reawakened.Players;

namespace Server.Reawakened.Network.Extensions;

public static class SendProtocols
{
    public static void SendXml(this Player player, string actionType, string message) =>
        player.NetState.SendXml(actionType, message);

    public static void SendXt(this Player player, string actionType, params object[] messages) =>
        player.NetState.SendXt(actionType, messages);

    public static void SendXt(this Player player, string actionType, params string[] messages) =>
        player.NetState.SendXt(actionType, messages);

    public static void SendXml(this NetState state, string actionType, string message) =>
        state.Send(
            $"<msg t=\"sys\"><body action='{actionType}' r='{state.GetLevelId()}'>{message}</body></msg>", actionType
        );

    public static void SendXt(this NetState state, string actionType, params object[] messages) =>
        state.SendXt(actionType, [.. messages.Select(x => x == null ? string.Empty : x.ToString())]);

    public static void SendXt(this NetState state, string actionType, params string[] messages) =>
        state.Send(
            $"%xt%{actionType}%{state.GetLevelId()}%{string.Join('%', messages)}%", actionType
        );

    private static int GetLevelId(this NetState state) =>
        state.Get<Player>()?.Room?.LevelInfo.LevelId ?? -1;
}
