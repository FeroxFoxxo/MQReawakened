using Server.Base.Network;
using Server.Reawakened.Players;

namespace Server.Reawakened.Network.Extensions;

public static class SendProtocols
{
    public static void SendXml(this NetState state, string actionType, string message) =>
        state.Send(
            $"<msg t=\"sys\"><body action='{actionType}' r='{state.GetLevelId()}'>{message}</body></msg>", actionType
        );

    public static void SendXt(this NetState state, string actionType, params object[] messages) =>
        state.SendXt(actionType, messages.Select(x => x.ToString()).ToArray());

    public static void SendXt(this NetState state, string actionType, params string[] messages) =>
        state.Send(
            $"%xt%{actionType}%{state.GetLevelId()}%{string.Join('%', messages)}%", actionType
        );

    private static int GetLevelId(this NetState state) =>
        state.Get<Player>()?.CurrentRoom?.LevelInfo.LevelId ?? -1;
}
