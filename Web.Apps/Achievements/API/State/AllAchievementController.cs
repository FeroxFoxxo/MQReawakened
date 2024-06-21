using Achievement.CharacterData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Web.Apps.Achievements.API.State;
[Route("Apps/achievements/api/state/{uuid}/{characterId}/achievements/all")]
public class AllAchievementController(CharacterHandler characterHandler,
    InternalAchievement internalAchievement) : Controller
{
    [HttpGet]
    public IActionResult GetAchievements([FromRoute] string uuid, [FromRoute] string characterId)
    {
        var _uuid = int.Parse(uuid);
        var _characterId = int.Parse(characterId);

        var character = characterHandler.GetCharacterFromId(_characterId);

        if (character == null)
            return NotFound();

        if (character.UserUuid != _uuid)
            return Forbid();

        var achievements = character.GetAllAchievements(internalAchievement);

        var points = 0;

        foreach (var compAch in achievements.Where(a => a.conditions.All(c => c.value == c.completionCount)))
            points += internalAchievement.Definitions.achievements.First(a => a.id == compAch.id).points;

        var ach = new AllCharacterAchievements()
        {
            achievements = achievements,
            points = points,
            status = true
        };

        return Ok(JsonConvert.SerializeObject(ach));
    }
}
