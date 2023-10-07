namespace Server.Base.Network.Models;

public class ProtocolResponse(string packetId, bool isUnhandled)
{
    public string ProtocolId { get; } = packetId;
    public bool IsUnhandled { get; } = isUnhandled;
}
