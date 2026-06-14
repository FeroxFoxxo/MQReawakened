using LitJson;
using Microsoft.AspNetCore.Mvc;
using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.XMLs.Bundles.Internal;
using Web.Apps.Leaderboards.Data;
using Web.Apps.Leaderboards.Database.Scores;

namespace Web.Apps.Leaderboards.API.Scores;
[Route("Apps/leaderboards/api/top/scores/{gameId}")]
public class TopScoresController(CharacterHandler characterHandler, TopScoresHandler topScoresHandler,
    InternalLeaderboards leaderboards, ServerRConfig rConfig) : Controller
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
            ["game"] = new JsonData
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
        
        var allScores = new List<TopScore>();
        var dailyScores = new List<TopScore>();
        var weeklyScores = new List<TopScore>();

        if (topScores != null)
        {
            var topScoresList = topScores.Scores.DeepCopy();
            var sortedScores = SortScores(game, topScoresList);

            var hasChanges = false;

            var allRank = 1;
            var weeklyRank = 1;
            var dailyRank = 1;
            foreach (var score in sortedScores)
            {
                var character = characterHandler.GetCharacterFromId(score.CharacterId);

                if (character == null)
                {
                    topScores.Scores.Remove(score);
                    hasChanges = true;
                    continue;
                }
                
                var charJson = new JsonData
                {
                    ["id"] = character.Id,
                    ["name"] = character.CharacterName,
                    ["gender"] = (short)character.Gender,
                    ["level"] = (short)character.GlobalLevel,
                    ["tribe"] = Enum.GetName(character.Allegiance)
                };

                if (allScores.All(x => x.CharacterId != character.Id)
                    && dailyScores.All(x => x.CharacterId != character.Id)
                    && weeklyScores.All(x => x.CharacterId != character.Id))
                    topScoresObject["characters"].Add(charJson);
                
                var scoreJson = new JsonData
                {
                    ["score"] = score.Score,
                    ["rank"] = score.Rank,
                    ["characterId"] = score.CharacterId,
                    ["time"] = score.Time
                };

                if (allScores.All(x => x.CharacterId != score.CharacterId))
                {
                    scoreJson["rank"] = allRank;
                    topScoresObject["scores"]["alltime"].Add(scoreJson);
                    allRank++;
                    allScores.Add(score);
                }
                
                var dateTime = DateTime.ParseExact(score.Time, "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz", null);

                var timeDiff = dateTime - DateTime.Now;
                
                if (weeklyScores.All(x => x.CharacterId != score.CharacterId)
                    && timeDiff.TotalDays < 7 && dateTime.Month == DateTime.Now.Month)
                {
                    scoreJson["rank"] = weeklyRank;
                    topScoresObject["scores"]["week"].Add(scoreJson);
                    weeklyRank++;
                    weeklyScores.Add(score);
                }
                
                if (dailyScores.All(x => x.CharacterId != score.CharacterId)
                    && dateTime.Day == DateTime.Now.Day)
                {
                    scoreJson["rank"] = dailyRank;
                    topScoresObject["scores"]["day"].Add(scoreJson);
                    dailyRank++;
                    dailyScores.Add(score);
                }
            }

            if (hasChanges)
                topScoresHandler.Update(topScores.Write);
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
