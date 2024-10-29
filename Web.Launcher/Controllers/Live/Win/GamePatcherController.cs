using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.Live.Win;

[Route("live/game/win32/{gameVersion}")]
public class GamePatcherController(LoadUpdates loadUpdates, ILogger<GamePatcherController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetFile([FromRoute] string gameVersion)
    {
        gameVersion = gameVersion.Replace(".zip", "");

        if (loadUpdates.WinClientFiles.TryGetValue(gameVersion, out var path))
        {
            var memory = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
                await stream.CopyToAsync(memory);

            memory.Position = 0;

            logger.LogInformation("Downloading patch version: {GameVersion} at path {Path}", gameVersion, path);
            return File(memory, "application/octet-stream", gameVersion + ".zip");
        }
        else
            return NotFound();
    }
}
