using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Base.Core.Services;
using Server.Base.Database.Accounts;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Network.Services;
using Web.Launcher.Extensions;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/authenticate")]
public class AuthenticateController(AccountHandler accHandler, UserInfoHandler userInfoHandler,
    TemporaryDataStorage temporaryDataStorage, RandomKeyGenerator keyGenerator,
    GetServerAddress getSA, LauncherRwConfig config, LauncherRConfig rConfig) : Controller
{
    [HttpPost]
    public IActionResult GetLoginInfo([FromForm] string username, [FromForm] string token)
    {
        username = username?.Trim();
        token = token?.Trim();

        var account = accHandler.GetAccountFromUsername(username);

        if (account == null)
            return Unauthorized();

        var userInfo = userInfoHandler.GetUserFromId(account.Id);

        if (userInfo == null)
            return Unauthorized();

        if (userInfo.AuthToken != token)
            return Unauthorized();

        var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(account.Id.ToString());

        temporaryDataStorage.AddData(sId, userInfo.Write);
        temporaryDataStorage.AddData(sId, account.Write);

        var loginData = account.GetLoginData(userInfo, getSA, config, rConfig);

        return Ok(JsonConvert.SerializeObject(loginData));
    }
}
