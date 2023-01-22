using Server.Base.Network;
using Server.Reawakened.Core.Network.Extensions;

namespace Server.Reawakened.Core.Network.Protocols;

public abstract class BaseProtocol
{
    public NetState NetState;

    public abstract string ProtocolName { get; }

    public void InitializeProtocol(NetState state) => NetState = state;

    public void SendXml(string actionType, string message) =>
        NetState.SendXml(actionType, message);

    public void SendXt(string actionType, params string[] messages) =>
        NetState.SendXt(actionType, messages);
}
