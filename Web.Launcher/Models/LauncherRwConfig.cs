using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.Launcher.Models;

public class LauncherRwConfig : IRwConfig
{
    public long LastClientUpdate { get; set; }
    public long v2014Timestamp { get; set; }
    public string GameSettingsFile { get; set; }
    public bool StartLauncherOnCommand { get; set; }
    public string AnalyticsApiKey { get; set; }

    public LauncherRwConfig()
    {
        LastClientUpdate = DateTime.Now.ToUnixTimestamp();
        v2014Timestamp = DateTime.Now.ToUnixTimestamp();
        StartLauncherOnCommand = false;
        GameSettingsFile = string.Empty;
        AnalyticsApiKey = string.Empty;
    }
}
