using Microsoft.AspNetCore.Mvc;
using Web.WebPlayer.Services;
using FileIO = System.IO.File;

namespace Web.WebPlayer.Controllers.API;

[Route("api/getGame/{gameVersion}")]
public class GetGame(LoadGameClients loadGameClients) : Controller
{
    [HttpGet]
    public IActionResult GetGameExecutable([FromRoute] string gameVersion) =>
        loadGameClients.ClientFiles.TryGetValue(gameVersion, out var path)
            ? new FileContentResult(FileIO.ReadAllBytes(path), "application/octet-stream")
            : (IActionResult)new FileNotFoundException();
}
