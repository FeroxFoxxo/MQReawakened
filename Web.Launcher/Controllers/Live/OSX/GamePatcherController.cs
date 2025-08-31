using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.Live.OSX;

[Route("live/game/osx/{gameVersion}")]
public class GamePatcherController(LoadUpdates loadUpdates, ILogger<GamePatcherController> logger) : Controller
{
    [HttpGet]
    public IActionResult GetFile([FromRoute] string gameVersion)
    {
        gameVersion = gameVersion.Replace(".zip", "");

        if (loadUpdates.OSXClientFiles.TryGetValue(gameVersion, out var path))
        {
            logger.LogInformation("Downloading patch version: {GameVersion} at path {Path}", gameVersion, path);
            return PhysicalFile(path, "application/octet-stream", enableRangeProcessing: true);
        }
        else
            return NotFound();
    }
}
