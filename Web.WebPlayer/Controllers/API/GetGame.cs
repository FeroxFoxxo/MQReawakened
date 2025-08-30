using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web.WebPlayer.Services;
using FileIO = System.IO.File;

namespace Web.WebPlayer.Controllers.API;

[Route("api/getGame/{gameVersion}")]
public class GetGame(LoadGameClients loadGameClients, ILogger<GetGame> logger) : Controller
{
    [HttpGet]
    public IActionResult GetGameExecutable([FromRoute] string gameVersion)
    {
        if (loadGameClients.ClientFiles.TryGetValue(gameVersion, out var path))
        {
            logger.LogInformation("Returning game version: {GameVersion} at path {Path}", gameVersion, path);
            return PhysicalFile(path, "application/octet-stream", enableRangeProcessing: true);
        }

        return new NotFoundResult();
    }
}
