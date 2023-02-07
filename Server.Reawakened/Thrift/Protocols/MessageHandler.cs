using Microsoft.Extensions.Logging;
using Server.Reawakened.Thrift.Abstractions;

namespace Server.Reawakened.Thrift.Protocols;

public class MessageHandler : ThriftHandler
{
    public MessageHandler(ILogger<MessageHandler> logger) : base(logger)
    {
    }

    public override void AddProcesses(Dictionary<string, ProcessFunction> processes)
    {
    }
}
