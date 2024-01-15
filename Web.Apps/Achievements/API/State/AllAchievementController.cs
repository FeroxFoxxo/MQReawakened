using Achievement.CharacterData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Base.Core.Extensions;
using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs.BundlesInternal;

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

        if (!characterHandler.Data.TryGetValue(_characterId, out var character))
            return NotFound();

        if (character.Data.UserUuid != _uuid)
            return NotFound();

        var random = new Random();

        var ach = new AllCharacterAchievements()
        {
            achievements = internalAchievement.Definitions.achievements.Select(x => new CharacterAchievement()
            {
                characterId = _characterId,
                id = x.id,

                conditions = x.conditions.Select(c => new CharacterCondition()
                {
                    id = c.id,
                    characterId = _characterId,

                    // GOAL TO COMPLETE
                    completionCount = c.goal,

                    // CURRENT PROGRESS
                    value = random.Next(0),

                    // UNUSED
                    ctime = long.MinValue, // COMPLETION TIME
                    mtime = long.MinValue, // MODIFIED TIME
                }).ToList(),

                // UNUSED
                ctime = long.MinValue,
                mtime = long.MinValue,
            }).ToList(),

            // INCREMENT BY ACHIEVEMENT POINTS
            points = 0,
            status = true // UNUSED
        };

        return Ok(JsonConvert.SerializeObject(ach));
    }
}
