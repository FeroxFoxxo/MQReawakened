using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Services;
using Server.Reawakened.Players.Services;
using System.Dynamic;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/login")]
public class LoginController(AccountHandler accHandler, UserInfoHandler userInfoHandler,
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
            return Unauthorized();

        dynamic resp = new ExpandoObject();
        resp.status = true;

        dynamic user = new ExpandoObject();
        user.premium = userInfo.Member;

        dynamic local = new ExpandoObject();
        local.uuid = account.Id.ToString();
        local.username = account.Username;
        local.createdTime = ((DateTimeOffset)account.Created).ToUnixTimeSeconds();
        user.local = local;

        dynamic sso = new ExpandoObject();
        sso.authToken = userInfo.AuthToken;
        sso.gender = Enum.GetName(userInfo.Gender)!;
        sso.dob = userInfo.DateOfBirth;
        user.sso = sso;

        resp.user = user;

        dynamic analytics = new ExpandoObject();
        analytics.id = rConfig.AnalyticsId.ToString();
        analytics.trackingShortId = userInfo.TrackingShortId;
        analytics.enabled = rConfig.AnalyticsEnabled;
        analytics.firstTimeLogin = account.Created == account.LastLogin ? "true" : "false";
        analytics.firstLoginToday = (DateTime.UtcNow - account.LastLogin).TotalDays >= 1;
        analytics.baseUrl = $"{rConfig.ServerBaseUrl1}/Analytics";
        analytics.apiKey = config.AnalyticsApiKey;
        resp.analytics = analytics;

        dynamic additional = new ExpandoObject();
        additional.signupExperience = userInfo.SignUpExperience;
        additional.region = userInfo.Region;
        resp.additional = additional;

        return Ok(resp);
    }
}
