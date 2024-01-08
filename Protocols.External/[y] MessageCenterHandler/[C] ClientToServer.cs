using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Thrift.Abstractions;
using Server.Reawakened.Thrift.Protocols;
using Thrift.Protocol;
using Thrift.Transport;

namespace Protocols.External._y__MessageCenterHandler;

public class ClientToServer : ExternalProtocol
{
    public override string ProtocolName => "yC";

    public MessageHandler MessageHandler { get; set; }

    public override void Run(string[] message)
    {
        var server2Client = new ChunkedTransport(delegate (string protocol) { SendXt("yC", protocol); });

        var s2CProtocol = new TCompactProtocol(server2Client);

        server2Client.SetChunk(message[5]);

        var protocol = new ThriftProtocol(s2CProtocol, s2CProtocol);

        MessageHandler.Process(protocol, NetState);
    }
}
