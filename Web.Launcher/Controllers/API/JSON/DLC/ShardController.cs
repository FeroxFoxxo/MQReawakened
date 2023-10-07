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
    private readonly AccountHandler _accHandler = accHandler;
    private readonly InternalRwConfig _config = config;
    private readonly RandomKeyGenerator _keyGenerator = keyGenerator;
    private readonly TemporaryDataStorage _temporaryDataStorage = temporaryDataStorage;
    private readonly UserInfoHandler _userInfoHandler = userInfoHandler;

    [HttpPost]
    public IActionResult GetShardInfo([FromForm] string username, [FromForm] string authToken, [FromForm] int uuid)
    {
        if (!_accHandler.Data.TryGetValue(uuid, out var account) || !_userInfoHandler.Data.TryGetValue(uuid, out var user))
            return Unauthorized();

        if (account.Username != username || user.AuthToken != authToken)
            return Unauthorized();

        var sId = _keyGenerator.GetRandomKey<TemporaryDataStorage>(uuid.ToString());

        _temporaryDataStorage.AddData(sId, user);
        _temporaryDataStorage.AddData(sId, account);

        var json = new JObject
        {
            { "status", true },
            {
                "sharder", new JObject
                {
                    { "unity.login.sid", sId },
                    { "unity.login.host", _config.GetHostName() }
                }
            }
        };

        return Ok(JsonConvert.SerializeObject(json));
    }
}
