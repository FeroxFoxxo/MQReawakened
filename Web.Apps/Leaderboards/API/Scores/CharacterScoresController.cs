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
            var gameId = GetGameId(score.Key);

            characterScores.Add(gameId.ToString(), new LeaderBoardCharacterScoresJson.Score
            {
                score = 1,
                time = score.Value.ToString()
            });
        }

        var characterScoresJson = new LeaderBoardCharacterScoresJson
        {
            status = true,
            scores = characterScores
        };
        
        return Ok(JsonMapper.ToJson(characterScoresJson));
    }

    private int GetGameId(string levelName) => levelName switch
    {
        "LV_CRS_MiniRace01" => 1,
        "LV_CRS_MiniRace02" => 2,
        "LV_CRS_MiniTorch01" => 3,
        "LV_CRS_MiniTorch02" => 4,
        "PetArenaBattleLevel" => 5,
        _ => 0,
    };
}
