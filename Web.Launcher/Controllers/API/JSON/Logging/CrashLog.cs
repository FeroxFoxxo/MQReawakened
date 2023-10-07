using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Services;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.Logging;

[Route("/api/json/logging/crash_log")]
public class CrashLog(ILogger<CrashLog> logger, ServerHandler handler, LauncherRConfig config) : Controller
{
    private readonly LauncherRConfig _config = config;
    private readonly ServerHandler _handler = handler;
    private readonly ILogger<CrashLog> _logger = logger;

    [HttpPost]
    public IActionResult PrintCrashReport([FromForm] string log)
    {
        _logger.LogCritical("Game Crash Log:");
        _logger.LogError("{Error}",
            string.Join('\n', log.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))));

        if (_config.CrashOnError)
            _handler.KillServer(false);

        return Ok();
    }
}
