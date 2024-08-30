using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.Live;

[Route("live/game/win32/{gameVersion}")]
public class PatcherController(LoadUpdates loadUpdates, ILogger<PatcherController> logger) : Controller
{
    [HttpGet]
    public IActionResult GetFile([FromRoute] string gameVersion)
    {
        gameVersion = gameVersion.Replace(".zip", "");

        if (loadUpdates.ClientFiles.TryGetValue(gameVersion, out var path))
        {
            var fileBytes = System.IO.File.ReadAllBytes(path);

            logger.LogInformation("Downloading patch version: {GameVersion} at path {Path}", gameVersion, path);
            return File(fileBytes, "application/zip", gameVersion + ".zip");
        }
        else
        {
            return NotFound();
        }
    }
}
