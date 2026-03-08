using LitJson;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.XMLs.Bundles.Internal;
using Web.Apps.Leaderboards.Data;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Services;

namespace Web.Apps.Leaderboards.API.Scores;
[Route("Apps/leaderboards/api/top/scores/{gameId}")]
public class TopScoresController(CharacterHandler characterHandler, TopScoresHandler topScoresHandler, ILogger<TopScoresController> logger, InternalLeaderboards leaderboards,
    ServerRConfig rConfig, LeaderboardHandler handler) : Controller
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

        var topScoresObject = new JsonData
        {
            ["status"] = true,
            ["characters"] = NewArray(),
            ["game"] = new JsonData()
            {
                ["id"] = game.id,
                ["name"] = game.name,
                ["sortDirection"] = game.sortDirection,
                ["scoreType"] = game.scoreType,
                ["maxScores"] = game.maxScores
            },
            ["scores"] = new JsonData
            {
                ["day"] = NewArray(),
                ["week"] = NewArray(),
                ["alltime"] = NewArray()
            }
        };

        if (rConfig.GameVersion >= GameVersion.vPetMasters2014)
            topScoresObject["game"]["ranked"] = game.ranked;

        var topScores = topScoresHandler.GetScoresFromId(_gameId);

        if (topScores != null)
        {
            var topScoresList = topScores.Scores.DeepCopy();
            var sortedScores = SortScores(game, topScoresList);

            foreach (var score in sortedScores)
            {
                var character = characterHandler.GetCharacterFromId(score.CharacterId);

                var charJson = new JsonData
                {
                    ["id"] = character.Id,
                    ["name"] = character.CharacterName,
                    ["gender"] = (short)character.Gender,
                    ["level"] = (short)character.GlobalLevel,
                    ["tribe"] = Enum.GetName(character.Allegiance)
                };

                topScoresObject["characters"].Add(charJson);
            }

            var rank = 1;
            foreach (var score in sortedScores)
            {
                var scoreJson = new JsonData
                {
                    ["score"] = score.Score,
                    ["rank"] = rank,
                    ["characterId"] = score.CharacterId,
                    ["time"] = score.Time
                };

                topScoresObject["scores"]["alltime"].Add(scoreJson);

                rank++;
            }
        }

        return Ok(JsonMapper.ToJson(topScoresObject));
    }

    private JsonData NewArray()
    {
        var arrayJson = new JsonData();
        arrayJson.SetJsonType(JsonType.Array);
        return arrayJson;
    }

    private List<TopScore> SortScores(LeaderBoardGameJson.Game game, List<TopScore> scores) =>
        game.sortDirection == "DESC" ? [.. scores.OrderByDescending(x => x.Score)] : [.. scores.OrderBy(x => x.Score)];
}
