namespace Server.Base.Network.Models;

public class ProtocolResponse(string packetId, bool isUnhandled, object packetData)
{
    public string ProtocolId { get; } = packetId;
    public bool IsUnhandled { get; } = isUnhandled;
    public object PacketData { get; } = packetData;
}
