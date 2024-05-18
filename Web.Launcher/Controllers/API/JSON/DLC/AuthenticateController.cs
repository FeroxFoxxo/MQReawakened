using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Server.Base.Accounts.Services;
using Server.Base.Core.Services;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Services;
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

        var account = accHandler.GetInternal().FirstOrDefault(x => x.Value.Username == username).Value;

        if (account == null)
            return Unauthorized();

        var userInfo = userInfoHandler.Get(account.Id);

        if (userInfo == null)
            return Unauthorized();

        if (userInfo.AuthToken != token)
            return Unauthorized();

        var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(account.Id.ToString());

        temporaryDataStorage.AddData(sId, userInfo);
        temporaryDataStorage.AddData(sId, account);

        var loginData = account.GetLoginData(userInfo, getSA, config, rConfig);

        return Ok(JsonConvert.SerializeObject(loginData));
    }
}
