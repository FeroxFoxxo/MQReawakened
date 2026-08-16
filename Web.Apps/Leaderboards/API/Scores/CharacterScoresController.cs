using LitJson;
using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.XMLs.Bundles.Internal;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Services;

namespace Web.Apps.Leaderboards.API.Scores;

[Route("Apps/leaderboards/api/character/{uuid}/{characterId}/scores")]
public class CharacterScoresController(InternalLeaderboards leaderboards, CharacterHandler characterHandler,
    TopScoresHandler topScoresHandler, LeaderboardHandler leaderboardHandler) : Controller
{
    [HttpGet]
    public IActionResult GetScores([FromRoute] string uuid, [FromRoute] string characterId)
    {
        var _uuid = int.Parse(uuid);
        var _characterId = int.Parse(characterId);

        if (!leaderboardHandler.CharacterCache.TryGetValue(_characterId, out var character))
        {
            character = characterHandler.GetCharacterFromId(_characterId);
            leaderboardHandler.CharacterCache[_characterId] = character;
        }
        
        if (character == null)
            return NotFound();

        if (character.UserUuid != _uuid)
            return Forbid();

        var characterScores = new JsonData
        {
            ["status"] = true
        };

        var scores = new JsonData();

        foreach (var game in leaderboards.Games)
        {
            var topScore = topScoresHandler.GetScoresFromId(game.id);
            
            if (topScore == null)
                continue;

            var characterScore = topScore.Scores
                .FirstOrDefault(x => x.CharacterId == character.Id);

            if (characterScore == null)
                continue;

            scores[game.id.ToString()] = new JsonData
            {
                ["score"] = characterScore.Score,
                ["time"] = characterScore.Time
            };
        }

        characterScores["scores"] = scores;
        
        return Ok(JsonMapper.ToJson(characterScores));
    }
}
