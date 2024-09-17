using LitJson;
using Microsoft.AspNetCore.Mvc;
using Web.Apps.Leaderboards.Services;

namespace Web.Apps.Leaderboards.API;

[Route("Apps/leaderboards/api/Games")]
public class GamesController(LeaderboardHandler handler) : Controller
{
    [HttpGet]
    public IActionResult GetGames() => Ok(JsonMapper.ToJson(handler.Games));
}
