using Server.Base.Logging;
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
    public FileLogger Logger { get; set; }

    public override void Run(string[] message)
    {
        var server2Client = new ChunkedTransport(delegate (string protocol) { SendXt("dC", protocol); });

        var s2CProtocol = new TCompactProtocol(server2Client);

        try
        {
            server2Client.SetChunk(message[5]);
        }
        catch (FormatException)
        {
            Logger.WriteGenericLog<ClientToServer>("base64-errors", "Description Handler Client Protocol",
                $"Could not process client protocol: '{message[5]}'", LoggerType.Trace);
            return;
        }

        var protocol = new ThriftProtocol(s2CProtocol, s2CProtocol);

        DescriptionHandler.Process(protocol, NetState);
    }
}
