using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.Live.Win;

[Route("live/game/win32/{gameVersion}")]
public class GamePatcherController(LoadUpdates loadUpdates, ILogger<GamePatcherController> logger) : Controller
{
    [HttpGet]
    public IActionResult GetFile([FromRoute] string gameVersion)
    {
        gameVersion = gameVersion.Replace(".zip", "");

        if (loadUpdates.WinClientFiles.TryGetValue(gameVersion, out var path))
        {
            logger.LogInformation("Downloading patch version: {GameVersion} at path {Path}", gameVersion, path);
            return PhysicalFile(path, "application/octet-stream", enableRangeProcessing: true);
        }
        else
            return NotFound();
    }
}
