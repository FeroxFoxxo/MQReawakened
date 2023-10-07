using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Services;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.Logging;

[Route("/api/json/logging/crash_log")]
public class CrashLog(ILogger<CrashLog> logger, ServerHandler handler, LauncherRConfig config) : Controller
{
    [HttpPost]
    public IActionResult PrintCrashReport([FromForm] string log)
    {
        logger.LogCritical("Game Crash Log:");
        logger.LogError("{Error}",
            string.Join('\n', log.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))));

        if (config.CrashOnError)
            handler.KillServer(false);

        return Ok();
    }
}
