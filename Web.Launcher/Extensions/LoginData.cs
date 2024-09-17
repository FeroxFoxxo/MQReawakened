using LitJson;
using Server.Base.Core.Configs;
using Server.Base.Database.Accounts;
using Server.Reawakened.Database.Users;
using Web.Launcher.Models;

namespace Web.Launcher.Extensions;
public static class LoginData
{
    public static string GetLoginData(this AccountModel account, UserInfoModel userInfo,
        InternalRwConfig iWConfig, LauncherRwConfig config, LauncherRConfig rConfig) =>
        JsonMapper.ToJson(
            new JsonData()
            {
                ["status"] = true,

                ["analytics"] = new JsonData
                {
                    ["id"] = rConfig.AnalyticsId.ToString(),
                    ["trackingShortId"] = userInfo.TrackingShortId,
                    ["enabled"] = rConfig.AnalyticsEnabled,
                    ["firstLoginToday"] = (DateTime.UtcNow - account.LastLogin).TotalDays >= 1,
                    ["baseUrl"] = $"{iWConfig.GetHostAddress()}/Analytics",
                    ["apiKey"] = config.AnalyticsApiKey,
                    ["firstTimeLogin"] = account.Created == account.LastLogin ? "true" : "false"
                },

                ["additional"] = new JsonData
                {
                    ["signupExperience"] = userInfo.SignUpExperience,
                    ["region"] = userInfo.Region
                },

                ["user"] = new JsonData
                {
                    ["local"] = new JsonData
                    {
                        ["uuid"] = account.Id.ToString(),
                        ["username"] = account.Username,
                        ["createdTime"] = ((DateTimeOffset)account.Created).ToUnixTimeSeconds()
                    },

                    ["sso"] = new JsonData
                    {
                        ["authToken"] = userInfo.AuthToken,
                        ["gender"] = Enum.GetName(userInfo.Gender),
                        ["dob"] = userInfo.DateOfBirth.ToString()
                    },

                    ["premium"] = new JsonData
                    {
                        ["membership"] = userInfo.Member
                    }
                }
            }
        );
}
