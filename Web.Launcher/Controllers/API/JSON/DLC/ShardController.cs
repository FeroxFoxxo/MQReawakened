using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Base.Accounts.Services;
using Server.Base.Core.Configs;
using Server.Base.Core.Services;
using Server.Reawakened.Network.Services;
using Server.Reawakened.Players.Services;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/shard")]
public class ShardController(AccountHandler accHandler, UserInfoHandler userInfoHandler,
    TemporaryDataStorage temporaryDataStorage,
    RandomKeyGenerator keyGenerator, InternalRwConfig config) : Controller
{
    [HttpPost]
    public IActionResult GetShardInfo([FromForm] string username, [FromForm] string authToken, [FromForm] int uuid)
    {
        username = username?.Trim();
        authToken = authToken?.Trim();

        var account = accHandler.Get(uuid);
        var user = userInfoHandler.Get(uuid);

        if (account == null || user == null)
            return Unauthorized();

        if (account.Username != username || user.AuthToken != authToken)
            return Unauthorized();

        var sId = keyGenerator.GetRandomKey<TemporaryDataStorage>(uuid.ToString());

        temporaryDataStorage.AddData(sId, user);
        temporaryDataStorage.AddData(sId, account);

        var json = new JObject
        {
            { "status", true },
            {
                "sharder", new JObject
                {
                    { "unity.login.sid", sId },
                    { "unity.login.host", config.GetHostName() }
                }
            }
        };

        return Ok(JsonConvert.SerializeObject(json));
    }
}
