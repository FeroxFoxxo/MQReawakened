using Thrift.Protocol;

namespace Server.Reawakened.Thrift.Abstractions;

public class ThriftProtocol
{
    public int SequenceId;
    public string Name;
    public TProtocol InProtocol;
    public TProtocol OutProtocol;

    public ThriftProtocol(TProtocol inProtocol, TProtocol outProtocol)
    {
        var tMessage = inProtocol.ReadMessageBegin();

        SequenceId = tMessage.SeqID;
        Name = tMessage.Name;

        InProtocol = inProtocol;
        OutProtocol = outProtocol;
    }
}
