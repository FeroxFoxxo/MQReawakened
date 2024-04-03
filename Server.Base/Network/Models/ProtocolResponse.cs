namespace Server.Base.Network.Models;

public class ProtocolResponse(string packetId, bool isUnhandled, object packetData)
{
    public string ProtocolId => packetId;
    public bool IsUnhandled => isUnhandled;
    public object PacketData => packetData;
}
