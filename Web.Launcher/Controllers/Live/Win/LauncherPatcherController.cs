using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.Live.Win;

[Route("live/launcher/win32/{launcherVersion}")]
public class LauncherPatcherController(LoadUpdates loadUpdates, ILogger<LauncherPatcherController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetFile([FromRoute] string launcherVersion)
    {
        launcherVersion = launcherVersion.Replace(".zip", "");

        if (loadUpdates.WinLauncherFiles.TryGetValue(launcherVersion, out var path))
        {
            var memory = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
                await stream.CopyToAsync(memory);

            memory.Position = 0;

            logger.LogInformation("Downloading patch version: {GameVersion} at path {Path}", launcherVersion, path);
            return File(memory, "application/octet-stream", launcherVersion + ".zip");
        }
        else
            return NotFound();
    }
}
