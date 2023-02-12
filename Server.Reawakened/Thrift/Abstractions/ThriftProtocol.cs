using Thrift.Protocol;

namespace Server.Reawakened.Thrift.Abstractions;

public class ThriftProtocol
{
    public TProtocol InProtocol;
    public string Name;
    public TProtocol OutProtocol;
    public int SequenceId;

    public ThriftProtocol(TProtocol inProtocol, TProtocol outProtocol)
    {
        var tMessage = inProtocol.ReadMessageBegin();

        SequenceId = tMessage.SeqID;
        Name = tMessage.Name;

        InProtocol = inProtocol;
        OutProtocol = outProtocol;
    }
}
