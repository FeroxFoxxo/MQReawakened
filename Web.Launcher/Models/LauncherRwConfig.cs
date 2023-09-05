using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.Launcher.Models;

public class LauncherRwConfig : IRwConfig
{
    public long LastClientUpdate { get; set; }
    public long MajorClientUpdate { get; set; }
    public string GameSettingsFile { get; set; }
    public bool Is2014Client { get; set; }
    public bool StartLauncherOnCommand { get; set; }
    public string AnalyticsApiKey { get; set; }

    public LauncherRwConfig()
    {
        Is2014Client = true;
        LastClientUpdate = DateTime.Now.ToUnixTimestamp();
        MajorClientUpdate = DateTime.Now.ToUnixTimestamp();
        StartLauncherOnCommand = false;
        GameSettingsFile = string.Empty;
        AnalyticsApiKey = string.Empty;
    }
}
