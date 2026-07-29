using LitJson;
using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Globalization;
using Web.Apps.Leaderboards.Data;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Services;

namespace Web.Apps.Leaderboards.API.Scores;
[Route("leaderboards/api/top/scores/{gameId}")]
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

        var topScores = topScoresHandler.GetScoresFromId(_gameId);

        if (topScores != null && topScores.Scores != null)
        {
            var sortedScores = SortScores(game, topScores.Scores);

            var now = DateTime.Now;
            var currentYear = now.Year;
            var currentDate = now.Date;
            var currentWeek = ISOWeek.GetWeekOfYear(now);

            var seenCharacters = new HashSet<int>();
            var allTimeChars = new HashSet<int>();
            var weeklyChars = new HashSet<int>();
            var dailyChars = new HashSet<int>();
            
            var characterCache = leaderboardHandler.CharacterCache; 
            var invalidCharacters = new HashSet<int>();

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
                    invalidCharacters.Add(score.CharacterId);
                    continue;
                }

                if (seenCharacters.Add(character.Id))
                {
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

                var dateTime = DateTime.ParseExact(score.Time, "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz", null);
                
                var scoreJson = new JsonData
                {
                    ["score"] = score.Score,
                    ["rank"] = score.Rank,
                    ["characterId"] = score.CharacterId,
                    ["time"] = score.Time
                };

                if (allTimeChars.Add(score.CharacterId))
                {
                    scoreJson["rank"] = allRank++;
                    topScoresObject["scores"]["alltime"].Add(scoreJson);
                    continue;
                }

                if (dateTime.Year == currentYear && ISOWeek.GetWeekOfYear(dateTime) == currentWeek)
                    if (weeklyChars.Add(score.CharacterId))
                    {
                        scoreJson["rank"] = weeklyRank++;
                        topScoresObject["scores"]["week"].Add(scoreJson);
                        continue;
                    }

                if (dateTime.Date == currentDate)
                    if (dailyChars.Add(score.CharacterId))
                    {
                        scoreJson["rank"] = dailyRank++;
                        topScoresObject["scores"]["day"].Add(scoreJson);
                        continue;
                    }
            }

            if (invalidCharacters.Count > 0)
            {
                topScores.Scores.RemoveAll(x => invalidCharacters.Contains(x.CharacterId));
                topScoresHandler.Update(topScores.Write);
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
