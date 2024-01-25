using Microsoft.AspNetCore.Mvc;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.Game.DLC.Live;

[Route("Game/dlc/live/current.txt")]
public class CurrentController(StartGame game) : Controller
{
    [HttpGet]
    public IActionResult GetCurrentData() => Ok(game.CurrentVersion);
}
