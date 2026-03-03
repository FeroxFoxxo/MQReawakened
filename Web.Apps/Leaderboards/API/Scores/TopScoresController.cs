using LitJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.XMLs.Bundles.Internal;
using Web.Apps.Leaderboards.Database.Scores;

namespace Web.Apps.Leaderboards.API.Scores;
[Route("Apps/leaderboards/api/top/scores/{gameId}")]
public class TopScoresController(CharacterHandler characterHandler,
    TopScoresHandler topScoresHandler, ILogger<TopScoresController> logger, InternalLeaderboards leaderboards) : Controller
{
    [HttpGet]
    public IActionResult GetScores([FromRoute] string gameId)
    {
        var _gameId = short.Parse(gameId);

        var game = leaderboards.Games.FirstOrDefault(x => x.id == _gameId);

        if (game == null)
            return NotFound();

        if (game.id != _gameId)
            return Forbid();

        var characters = new List<LeaderBoardTopScoresJson.Character>();
        var scores = new Dictionary<string, List<LeaderBoardTopScoresJson.Score>>
        {
            { "day", [] },
            { "week", [] },
            { "alltime", [] }
        };
        
        var topScores = topScoresHandler.GetScoresFromId(_gameId);

        if (topScores != null && topScores.GameId == game.id)
            foreach (var characterScore in topScores.Scores)
            {
                var character = characterHandler.GetCharacterFromId(characterScore.CharacterId);

                if (characters.FirstOrDefault(x => x.id == character.Id) != null) 
                    continue;
                
                characters.Add(new LeaderBoardTopScoresJson.Character
                {
                    id = character.Id,
                    name = character.CharacterName,
                    gender = (short)character.Gender,
                    level = (short)character.GlobalLevel,
                    tribe = character.Allegiance.ToString()
                });

                scores["alltime"].Add(
                    new LeaderBoardTopScoresJson.Score
                    {
                        score = characterScore.Score,
                        rank = characterScore.Rank,
                        characterId = character.Id,
                        time = characterScore.Time
                    }
                );
            }

        var topScoresJson = new LeaderBoardTopScoresJson
        {
            status = true,
            characters = characters,
            game = game,
            scores = scores
        };

        return Ok(JsonMapper.ToJson(topScoresJson));
    }
}
