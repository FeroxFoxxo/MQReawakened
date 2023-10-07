using Microsoft.Extensions.Logging;
using Server.Reawakened.Thrift.Abstractions;

namespace Server.Reawakened.Thrift.Protocols;

public class MessageHandler(ILogger<MessageHandler> logger) : ThriftHandler(logger)
{
    public override void AddProcesses(Dictionary<string, ProcessFunction> processes)
    {
    }
}
