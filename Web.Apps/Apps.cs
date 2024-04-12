using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.Apps;

public class Apps(ILogger<Apps> logger) : WebModule(logger)
{
}
