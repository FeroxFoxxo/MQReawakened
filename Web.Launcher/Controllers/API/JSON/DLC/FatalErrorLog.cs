using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("/api/json/dlc/fatalErrorLog")]
public class FatalErrorLog(ILogger<FatalErrorLog> logger) : Controller
{
    [HttpGet]
    public IActionResult OutputFatalCrash([FromQuery] string username)
    {
        username = username.Sanitize();

        logger.LogCritical("User '{Username}' encountered a fatal crash!", username);

        return Ok();
    }
}
