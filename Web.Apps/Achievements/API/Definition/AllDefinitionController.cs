using LitJson;
using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Web.Apps.Achievements.API.Definition;

[Route("Apps/achievements/api/definition/all")]
public class AllDefinitionController(InternalAchievement achievements) : Controller
{
    [HttpGet]
    public IActionResult GetAchievements() => Ok(JsonMapper.ToJson(achievements.Definitions));
}
