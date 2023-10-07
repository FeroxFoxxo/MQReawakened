using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Web.Apps.Leaderboards.Services;

namespace Web.Apps.Leaderboards.API;

[Route("Apps/leaderboards/api/Games")]
public class GamesController(LeaderboardHandler handler) : Controller
{
    private readonly LeaderboardHandler _handler = handler;

    [HttpGet]
    public IActionResult GetGames() => Ok(JsonConvert.SerializeObject(_handler.Games));
}
