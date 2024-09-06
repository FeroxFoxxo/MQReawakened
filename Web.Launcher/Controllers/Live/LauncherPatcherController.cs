using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.Live;

[Route("live/launcher/win32/{launcherVersion}")]
public class LauncherPatcherController(LoadUpdates loadUpdates, ILogger<LauncherPatcherController> logger) : Controller
{
    [HttpGet]
    public IActionResult GetFile([FromRoute] string launcherVersion)
    {
        launcherVersion = launcherVersion.Replace(".zip", "");

        if (loadUpdates.LauncherFiles.TryGetValue(launcherVersion, out var path))
        {
            var fileBytes = System.IO.File.ReadAllBytes(path);

            logger.LogInformation("Downloading patch version: {LauncherVersion} at path {Path}", launcherVersion, path);
            return File(fileBytes, "application/zip", launcherVersion + ".zip");
        }
        else
            return NotFound();
    }
}
