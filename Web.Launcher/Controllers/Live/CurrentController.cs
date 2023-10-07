using Microsoft.AspNetCore.Mvc;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.Live;

[Route("live/current.txt")]
public class CurrentController(StartGame game) : Controller
{
    private readonly StartGame _game = game;

    [HttpGet]
    public IActionResult GetCurrentData() => Ok(_game.CurrentVersion);
}
