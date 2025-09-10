using Server.Base.Network;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;

namespace Server.Reawakened.Network.Protocols;

public abstract class BaseProtocol
{
    public NetState NetState;

    private Player _player;
    public Player Player => _player ??= NetState.Get<Player>();

    public abstract string ProtocolName { get; }

    public void InitializeProtocol(NetState state) => NetState = state;

    public void SendXml(string actionType, string message) =>
        NetState.SendXml(actionType, message);

    public void SendXt(string actionType) =>
        NetState.SendXt(actionType, []);

    public void SendXt(string actionType, params object[] messages) =>
        NetState.SendXt(actionType, messages);

    public void SendXt(string actionType, params string[] messages) =>
        NetState.SendXt(actionType, messages);
}
