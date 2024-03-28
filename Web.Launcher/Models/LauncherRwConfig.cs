using Server.Base.Core.Abstractions;

namespace Web.Launcher.Models;

public class LauncherRwConfig : IRwConfig
{
    public string GameSettingsFile { get; set; }
    public bool StartLauncherOnCommand { get; set; }
    public string AnalyticsApiKey { get; set; }

    public LauncherRwConfig()
    {
        StartLauncherOnCommand = false;
        GameSettingsFile = string.Empty;
        AnalyticsApiKey = string.Empty;
    }
}
