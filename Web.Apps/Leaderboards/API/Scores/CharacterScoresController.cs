using LitJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.XMLs.Bundles.Internal;
using Web.Apps.Leaderboards.Database.Scores;

namespace Web.Apps.Leaderboards.API.Scores;

[Route("Apps/leaderboards/api/character/{uuid}/{characterId}/scores")]
public class CharacterScoresController(InternalLeaderboards leaderboards, CharacterHandler characterHandler,
    TopScoresHandler topScoresHandler, ILogger<CharacterScoresController> logger) : Controller
{
    [HttpGet]
    public IActionResult GetScores([FromRoute] string uuid, [FromRoute] string characterId)
    {
        var _uuid = int.Parse(uuid);
        var _characterId = int.Parse(characterId);

        var character = characterHandler.GetCharacterFromId(_characterId);

        if (character == null)
            return NotFound();

        if (character.UserUuid != _uuid)
            return Forbid();

        var characterScores = new Dictionary<string, LeaderBoardCharacterScoresJson.Score>();

        foreach (var score in character.BestMinigameTimes)
        {
            var gameId = leaderboards.Games.FirstOrDefault(x => x.name == score.Key).id;

            var topScore = topScoresHandler.GetScoresFromId(gameId).Scores.FirstOrDefault(x => x.CharacterId == character.Id);

            if (topScore != null && !characterScores.ContainsKey(gameId.ToString()))
                characterScores[gameId.ToString()] = new LeaderBoardCharacterScoresJson.Score
                {
                    score = topScore.Score,
                    time = topScore.Time
                };
        }

        var characterScoresJson = new LeaderBoardCharacterScoresJson
        {
            status = true,
            scores = characterScores
        };
        
        return Ok(JsonMapper.ToJson(characterScoresJson));
    }
}
