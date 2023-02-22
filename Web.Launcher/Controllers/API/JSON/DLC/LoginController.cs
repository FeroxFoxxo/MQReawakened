using Microsoft.AspNetCore.Mvc;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Services;
using Server.Reawakened.Players.Services;
using System.Dynamic;
using Web.Launcher.Models;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("api/json/dlc/login")]
public class LoginController : Controller
{
    private readonly AccountHandler _accHandler;
    private readonly StartConfig _config;
    private readonly PasswordHasher _passwordHasher;
    private readonly LauncherStaticConfig _sConfig;
    private readonly UserInfoHandler _userInfoHandler;

    public LoginController(AccountHandler accHandler, UserInfoHandler userInfoHandler,
        LauncherStaticConfig sConfig, PasswordHasher passwordHasher, StartConfig config)
    {
        _accHandler = accHandler;
        _userInfoHandler = userInfoHandler;
        _sConfig = sConfig;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    [HttpPost]
    public IActionResult HandleLogin([FromForm] string username, [FromForm] string password)
    {
        var hashedPw = _passwordHasher.GetPassword(username, password);

        var account = _accHandler.Data.Values.FirstOrDefault(x => x.Username == username);

        if (account == null)
            return Unauthorized();

        var userInfo = _userInfoHandler.Data.Values.FirstOrDefault(x => x.UserId == account.UserId);

        if (userInfo == null)
            return Unauthorized();

        if (account.Password != hashedPw && userInfo.AuthToken != password)
            return Unauthorized();

        dynamic resp = new ExpandoObject();
        resp.status = true;

        dynamic user = new ExpandoObject();
        user.premium = userInfo.Member;

        dynamic local = new ExpandoObject();
        local.uuid = account.UserId.ToString();
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
        analytics.id = _sConfig.AnalyticsId.ToString();
        analytics.trackingShortId = userInfo.TrackingShortId;
        analytics.enabled = _sConfig.AnalyticsEnabled;
        analytics.firstTimeLogin = account.Created == account.LastLogin ? "true" : "false";
        analytics.firstLoginToday = (DateTime.UtcNow - account.LastLogin).TotalDays >= 1;
        analytics.baseUrl = $"{_sConfig.BaseUrl}/Analytics";
        analytics.apiKey = _config.AnalyticsApiKey;
        resp.analytics = analytics;

        dynamic additional = new ExpandoObject();
        additional.signupExperience = userInfo.SignUpExperience;
        additional.region = userInfo.Region;
        resp.additional = additional;

        return Ok(resp);
    }
}
