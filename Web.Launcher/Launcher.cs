using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.Launcher;

public class Launcher(ILogger<Launcher> logger) : WebModule(logger)
{
}
