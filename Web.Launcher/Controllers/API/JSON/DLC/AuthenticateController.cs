using Microsoft.AspNetCore.Mvc;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Database.Accounts;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Network.Services;
using Web.Launcher.Extensions;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/authenticate")]
public class AuthenticateController(AccountHandler accHandler, UserInfoHandler userInfoHandler,
    TemporaryDataStorage temporaryDataStorage, RandomKeyGenerator keyGenerator,
    InternalRwConfig iWConfig, LauncherRwConfig config, LauncherRConfig rConfig) : Controller
{
    [HttpPost]
    public IActionResult GetLoginInfo([FromForm] string username, [FromForm] string token)
    {
        username = username.Sanitize();

        var account = accHandler.GetAccountFromUsername(username);

        if (account == null)
            return Unauthorized();

        var userInfo = userInfoHandler.GetUserFromId(account.Id);

        if (userInfo == null)
            return Unauthorized();

        if (userInfo.AuthToken != token)
            return Unauthorized();

        var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(account.Id.ToString());

        temporaryDataStorage.AddData(sId, account.Write);

        return Ok(account.GetLoginData(userInfo, iWConfig, config, rConfig));
    }
}
