using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Players.Helpers;

namespace Web.Launcher.Controllers.API;

[Route("api/getHost")]
public class GetPlayerCount(PlayerContainer container) : Controller
{
    [HttpGet]
    public IActionResult GetPlayers() => Ok(container.GetAllPlayers);
}
