using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Base.Accounts.Services;
using Server.Base.Core.Configs;
using Server.Base.Core.Services;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Services;
using System;
using Web.Launcher.Extensions;
using Web.Launcher.Models;
using Web.Launcher.Services;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/authenticate")]
public class AuthenticateController(AccountHandler accHandler, UserInfoHandler userInfoHandler,
    TemporaryDataStorage temporaryDataStorage, RandomKeyGenerator keyGenerator,
    StartGame startGame, LauncherRwConfig config, LauncherRConfig rConfig) : Controller
{
    [HttpPost]
    public IActionResult GetLoginInfo([FromForm] string username, [FromForm] string token)
    {
        var account = accHandler.Data.FirstOrDefault(x => x.Value.Username == username).Value;

        if (account == null)
            return Unauthorized();

        if (!userInfoHandler.Data.TryGetValue(account.Id, out var userInfo))
            return Unauthorized();

        if (userInfo.AuthToken != token)
            return Unauthorized();

        var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(account.Id.ToString());

        temporaryDataStorage.AddData(sId, userInfo);
        temporaryDataStorage.AddData(sId, account);

        var loginData = account.GetLoginData(userInfo, startGame, config, rConfig);

        return Ok(JsonConvert.SerializeObject(loginData));
    }
}
