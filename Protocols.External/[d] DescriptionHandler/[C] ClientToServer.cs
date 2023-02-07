using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Thrift.Abstractions;
using Server.Reawakened.Thrift.Protocols;
using Thrift.Protocol;
using Thrift.Transport;

namespace Protocols.External._d__DescriptionHandler;

public class ClientToServer : ExternalProtocol
{
    public override string ProtocolName => "dC";

    public DescriptionHandler DescriptionHandler { get; set; }

    public override void Run(string[] message)
    {
        var server2Client = new ChunkedTransport(delegate (string protocol) { SendXt("dC", protocol); });

        var s2CProtocol = new TCompactProtocol(server2Client);

        server2Client.SetChunk(message[5]);

        var protocol = new ThriftProtocol(s2CProtocol, s2CProtocol);

        DescriptionHandler.Process(protocol, NetState);
    }
}
