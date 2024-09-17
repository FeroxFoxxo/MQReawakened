using LitJson;
using Microsoft.AspNetCore.Mvc;
using Server.Base.Core.Configs;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Database.Accounts;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Network.Services;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/shard")]
public class ShardController(AccountHandler accHandler, UserInfoHandler userInfoHandler,
    TemporaryDataStorage temporaryDataStorage,
    RandomKeyGenerator keyGenerator, InternalRwConfig config) : Controller
{
    [HttpPost]
    public IActionResult GetShardInfo([FromForm] string username, [FromForm] string authToken, [FromForm] int uuid)
    {
        username = username.Sanitize();

        var account = accHandler.GetAccountFromId(uuid);
        var user = userInfoHandler.GetUserFromId(uuid);

        if (account == null || user == null)
            return Unauthorized();

        if (account.Username != username || user.AuthToken != authToken)
            return Unauthorized();

        var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(uuid.ToString());

        temporaryDataStorage.AddData(sId, account.Write);

        return Ok(
            JsonMapper.ToJson(
                new JsonData
                {
                    ["status"] = true,
                    ["sharder"] = new JsonData
                    {
                        ["unity.login.sid"] = sId,
                        ["unity.login.host"] = config.GetHostName()
                    }
                }
            )
        );
    }
}
