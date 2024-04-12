using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;

namespace Protocols.System;

public class SysProtocol(ILogger<SysProtocol> logger) : Module(logger)
{
}
