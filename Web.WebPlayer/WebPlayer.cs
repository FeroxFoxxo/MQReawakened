using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.WebPlayer;

public class WebPlayer(ILogger<WebPlayer> logger) : WebModule(logger)
{
}
