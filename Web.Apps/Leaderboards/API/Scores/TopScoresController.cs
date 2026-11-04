using LitJson;
using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Globalization;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Services;

namespace Web.Apps.Leaderboards.API.Scores;
[Route("Apps/leaderboards/api/top/scores/{gameId}")]
public class TopScoresController(CharacterHandler characterHandler, TopScoresHandler topScoresHandler,
    InternalLeaderboards leaderboards, ServerRConfig rConfig, LeaderboardHandler leaderboardHandler) : Controller
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

        var topScores = topScoresHandler.GetScoresFromGame(_gameId);

        if (topScores != null)
        {
            var sortedScores = SortScores(game, topScores);

            var now = DateTime.UtcNow;
            var currentYear = now.Year;
            var currentDate = now.Date;
            var currentWeek = ISOWeek.GetWeekOfYear(now);

            var seenCharacters = new List<int>();
            var allTimeChars = new List<int>();
            var weeklyChars = new List<int>();
            var dailyChars = new List<int>();
            
            var characterCache = leaderboardHandler.CharacterCache; 

            var allRank = 1;
            var weeklyRank = 1;
            var dailyRank = 1;

            foreach (var score in sortedScores)
            {
                if (!characterCache.TryGetValue(score.CharacterId, out var character))
                {
                    character = characterHandler.GetCharacterFromId(score.CharacterId);
                    characterCache[score.CharacterId] = character;
                }

                if (character == null)
                {
                    characterCache.Remove(score.CharacterId);
                    continue;
                }

                if (!seenCharacters.Contains(character.Id))
                {
                    seenCharacters.Add(character.Id);
                    var charJson = new JsonData
                    {
                        ["id"] = character.Id,
                        ["name"] = character.CharacterName,
                        ["gender"] = (short)character.Gender,
                        ["level"] = (short)character.GlobalLevel,
                        ["tribe"] = Enum.GetName(character.Allegiance),
                    };
                    topScoresObject["characters"].Add(charJson);
                }

                var dateTime = score.Time;
                
                var scoreJson = new JsonData
                {
                    ["score"] = score.Score,
                    ["rank"] = score.Rank,
                    ["characterId"] = score.CharacterId,
                    ["time"] = score.Time.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz")
                };

                if (!allTimeChars.Contains(score.CharacterId))
                {
                    allTimeChars.Add(score.CharacterId);
                    scoreJson["rank"] = allRank++;
                    topScoresObject["scores"]["alltime"].Add(scoreJson);
                    continue;
                }

                if (dateTime.Year == currentYear && ISOWeek.GetWeekOfYear(dateTime) == currentWeek)
                    if (!weeklyChars.Contains(score.CharacterId))
                    {
                        weeklyChars.Add(score.CharacterId);
                        scoreJson["rank"] = weeklyRank++;
                        topScoresObject["scores"]["week"].Add(scoreJson);
                        continue;
                    }

                if (dateTime.Date == currentDate)
                    if (!dailyChars.Contains(score.CharacterId))
                    {
                        dailyChars.Add(score.CharacterId);
                        scoreJson["rank"] = dailyRank++;
                        topScoresObject["scores"]["day"].Add(scoreJson);
                        continue;
                    }
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

    private List<TopScoresModel> SortScores(LeaderBoardGameJson.Game game, List<TopScoresModel> scores) =>
        game.sortDirection == "DESC" ? [.. scores.OrderByDescending(x => x.Score)] : [.. scores.OrderBy(x => x.Score)];
}
