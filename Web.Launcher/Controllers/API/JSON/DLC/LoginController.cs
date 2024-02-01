using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Services;
using Server.Reawakened.Players.Services;
using System.Dynamic;
using System.Security.Cryptography;
using Web.Launcher.Extensions;
using Web.Launcher.Models;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/login")]
public class LoginController(AccountHandler accHandler, UserInfoHandler userInfoHandler, StartGame startGame,
    LauncherRConfig rConfig, PasswordHasher passwordHasher, LauncherRwConfig config, ILogger<LoginController> logger) : Controller
{
    [HttpPost]
    public IActionResult HandleLogin([FromForm] string username, [FromForm] string password)
    {
        var hashedPw = passwordHasher.GetPassword(username, password);

        var account = accHandler.Data.Values.FirstOrDefault(x => x.Username == username);

        if (account == null)
        {
            logger.LogError("Could not find account for {Username}", username);
            return BadRequest();
        }

        var userInfo = userInfoHandler.Data.Values.FirstOrDefault(x => x.Id == account.Id);

        if (userInfo == null)
        {
            logger.LogError("Could not find user info for {Username} (ID: {Id})", username, account.Id);
            return BadRequest();
        }

        if (account.Password != hashedPw && userInfo.AuthToken != password)
        {
            logger.LogError("Account password does not match for {Username} (ID: {Id})", username, account.Id);
            return Unauthorized();
        }

        var loginData = account.GetLoginData(userInfo, startGame, config, rConfig);

        return Ok(JsonConvert.SerializeObject(loginData));
    }
}
