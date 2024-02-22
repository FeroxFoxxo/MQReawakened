using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Players.Helpers;

namespace Web.WebPlayer.Controllers.API;

[Route("api/getPlayers")]
public class GetPlayerCount(PlayerContainer container) : Controller
{
    [HttpGet]
    public IActionResult GetPlayers() => Ok(container.GetPlayerCount());
}
