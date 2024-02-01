using Newtonsoft.Json.Linq;
using Server.Base.Accounts.Models;
using Server.Reawakened.Players.Models;
using Web.Launcher.Models;
using Web.Launcher.Services;

namespace Web.Launcher.Extensions;
public static class LoginData
{
    public static JObject GetLoginData(this Account account, UserInfo userInfo,
        StartGame startGame, LauncherRwConfig config, LauncherRConfig rConfig) =>
        new ()
        {
            { "status", true },
            {
                "analytics", new JObject
                {
                    { "id", rConfig.AnalyticsId.ToString() },
                    { "trackingShortId", userInfo.TrackingShortId },
                    { "enabled", rConfig.AnalyticsEnabled },
                    { "firstLoginToday", (DateTime.UtcNow - account.LastLogin).TotalDays >= 1 },
                    { "baseUrl", $"{startGame.ServerAddress}/Analytics" },
                    { "apiKey", config.AnalyticsApiKey },
                    { "firstTimeLogin", account.Created == account.LastLogin ? "true" : "false" },
                }
            },
            {
                "additional", new JObject
                {
                    { "signupExperience", userInfo.SignUpExperience },
                    { "region", userInfo.Region },
                }
            },
            {
                "user", new JObject
                {
                    {
                        "local", new JObject
                        {
                            { "uuid", account.Id.ToString() },
                            { "username", account.Username },
                            { "createdTime", ((DateTimeOffset)account.Created).ToUnixTimeSeconds() },
                        }
                    },
                    {
                        "sso", new JObject
                        {
                            { "authToken", userInfo.AuthToken },
                            { "gender", Enum.GetName(userInfo.Gender) },
                            { "dob", userInfo.DateOfBirth },
                        }
                    },
                    {
                        "premium", new JObject
                        {
                            { "membership", userInfo.Member },
                        }
                    }
                }
            }
        };
}
