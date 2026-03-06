using LitJson;
using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Web.Apps.Leaderboards.Services;

namespace Web.Apps.Leaderboards.API;

[Route("Apps/leaderboards/api/games")]
public class GamesController(LeaderboardHandler handler, ServerRConfig rConfig) : Controller
{
    [HttpGet]
    public IActionResult GetGames()
    {
        var gamesObject = new JsonData
        {
            ["status"] = true
        };

        var gamesArray = new JsonData();

        foreach (var game in handler.Games.games)
        {
            if (game.name == "PetBattleArenaLevel" && rConfig.GameVersion == GameVersion.vEarly2014)
                continue;

            var gameJson = new JsonData
            {
                ["id"] = game.id,
                ["name"] = game.name,
                ["sortDirection"] = game.sortDirection,
                ["scoreType"] = game.scoreType,
                ["maxScores"] = game.maxScores
            };

            if (rConfig.GameVersion >= GameVersion.vPetMasters2014)
                gameJson["ranked"] = game.ranked;

            gamesArray.Add(gameJson);
        }

        gamesObject["games"] = gamesArray;

        return Ok(JsonMapper.ToJson(gamesObject));
    }
}
