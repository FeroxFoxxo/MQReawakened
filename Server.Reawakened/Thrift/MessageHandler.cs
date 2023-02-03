using Microsoft.Extensions.Logging;
using Thrift.Protocol;

namespace Server.Reawakened.Thrift;

public class MessageHandler
{
    public delegate void ProcessFunction(int sequenceId, TProtocol inProtocol, TProtocol outProtocol);

    public readonly Dictionary<string, ProcessFunction> ProcessMap;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(ILogger<MessageHandler> logger)
    {
        ProcessMap = new Dictionary<string, ProcessFunction>();
        _logger = logger;
    }

    public void Process(TProtocol inProtocol, TProtocol outProtocol)
    {
        var tMessage = inProtocol.ReadMessageBegin();

        ProcessMap.TryGetValue(tMessage.Name, out var process);

        if (process == null)
        {
            TProtocolUtil.Skip(inProtocol, TType.Struct);
            inProtocol.ReadMessageEnd();
            _logger.LogError("Invalid Thrift method: '{Message}'.", tMessage.Name);
        }
        else
        {
            process(tMessage.SeqID, inProtocol, outProtocol);
        }
    }
}
