using Newtonsoft.Json.Linq;
using Server.Base.Core.Configs;
using Server.Base.Database.Accounts;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Database.Users;
using Web.Launcher.Models;

namespace Web.Launcher.Extensions;
public static class LoginData
{
    public static JObject GetLoginData(this AccountModel account, UserInfoModel userInfo,
        InternalRwConfig iWConfig, LauncherRwConfig config, LauncherRConfig rConfig) =>
        new()
        {
            { "status", true },
            {
                "analytics", new JObject
                {
                    { "id", rConfig.AnalyticsId.ToString() },
                    { "trackingShortId", userInfo.TrackingShortId },
                    { "enabled", rConfig.AnalyticsEnabled },
                    { "firstLoginToday", (DateTime.UtcNow - account.LastLogin).TotalDays >= 1 },
                    { "baseUrl", $"{iWConfig.GetHostAddress()}/Analytics" },
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
