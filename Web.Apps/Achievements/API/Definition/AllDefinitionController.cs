using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Web.Apps.Achievements.Services;

namespace Web.Apps.Achievements.API.Definition;

[Route("Apps/achievements/api/definition/all")]
public class AllDefinitionController(AchievementHandler handler) : Controller
{
    private readonly AchievementHandler _handler = handler;

    [HttpGet]
    public IActionResult GetAchievements() => Ok(JsonConvert.SerializeObject(_handler.Definitions));
}
