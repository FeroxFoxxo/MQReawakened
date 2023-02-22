using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.Launcher.Models;

public class StartConfig : IConfig
{
    public long LastClientUpdate { get; set; }
    public string GameSettingsFile { get; set; }
    public bool Is2014Client { get; set; }
    public bool StartLauncherOnCommand { get; set; }
    public string AnalyticsApiKey { get; set; }

    public StartConfig()
    {
        Is2014Client = true;
        LastClientUpdate = DateTime.Now.ToUnixTimestamp();
        StartLauncherOnCommand = false;
        GameSettingsFile = string.Empty;
        AnalyticsApiKey = string.Empty;
    }
}
