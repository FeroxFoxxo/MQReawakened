namespace Server.Base.Network.Models;

public class ProtocolResponse
{
    public string ProtocolId { get; }
    public bool IsUnhandled { get; }

    public ProtocolResponse(string packetId, bool isUnhandled)
    {
        ProtocolId = packetId;
        IsUnhandled = isUnhandled;
    }
}
