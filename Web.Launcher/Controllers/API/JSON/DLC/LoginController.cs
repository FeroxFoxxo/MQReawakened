using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    private readonly LauncherRwConfig _config;
    private readonly PasswordHasher _passwordHasher;
    private readonly LauncherRConfig _rConfig;
    private readonly UserInfoHandler _userInfoHandler;
    private readonly ILogger<LoginController> _logger;

    public LoginController(AccountHandler accHandler, UserInfoHandler userInfoHandler,
        LauncherRConfig rConfig, PasswordHasher passwordHasher, LauncherRwConfig config, ILogger<LoginController> logger)
    {
        _accHandler = accHandler;
        _userInfoHandler = userInfoHandler;
        _rConfig = rConfig;
        _passwordHasher = passwordHasher;
        _config = config;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult HandleLogin([FromForm] string username, [FromForm] string password)
    {
        var hashedPw = _passwordHasher.GetPassword(username, password);

        var account = _accHandler.Data.Values.FirstOrDefault(x => x.Username == username);

        if (account == null)
        {
            _logger.LogError("Could not find account for {Username}", username);
            return BadRequest();
        }

        var userInfo = _userInfoHandler.Data.Values.FirstOrDefault(x => x.UserId == account.UserId);

        if (userInfo == null)
        {
            _logger.LogError("Could not find user info for {Username} (ID: {Id})", username, account.UserId);
            return BadRequest();
        }

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
        analytics.id = _rConfig.AnalyticsId.ToString();
        analytics.trackingShortId = userInfo.TrackingShortId;
        analytics.enabled = _rConfig.AnalyticsEnabled;
        analytics.firstTimeLogin = account.Created == account.LastLogin ? "true" : "false";
        analytics.firstLoginToday = (DateTime.UtcNow - account.LastLogin).TotalDays >= 1;
        analytics.baseUrl = $"{_rConfig.ServerBaseUrl1}/Analytics";
        analytics.apiKey = _config.AnalyticsApiKey;
        resp.analytics = analytics;

        dynamic additional = new ExpandoObject();
        additional.signupExperience = userInfo.SignUpExperience;
        additional.region = userInfo.Region;
        resp.additional = additional;

        return Ok(resp);
    }
}
