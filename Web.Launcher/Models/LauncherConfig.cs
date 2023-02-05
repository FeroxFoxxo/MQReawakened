using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.Launcher.Models;

public class LauncherConfig : IConfig
{
    public string GameSettingsFile { get; set; }
    public string News { get; set; }

    public ulong AnalyticsId { get; set; }
    public bool AnalyticsEnabled { get; set; }
    public string AnalyticsApiKey { get; set; }

    public string BaseUrl { get; set; }
    public string HeaderFolderFilter { get; set; }

    public string ProjectName { get; set; }

    public bool CrashOnError { get; set; }
    public bool LogAssets { get; set; }
    public bool DisableVersions { get; set; }
    public string CacheLicense { get; set; }

    public int CacheVersion { get; set; }
    public int CacheSize { get; set; }
    public int CacheExpiration { get; set; }

    public bool OverwriteGameConfig { get; set; }

    public bool Is2014Client { get; set; }
    public string TimeFilter { get; set; }
    public string OldClientLastUpdate { get; set; }
    public long LastClientUpdate { get; set; }

    public bool StartLauncherOnCommand { get; set; }

    public LauncherConfig()
    {
        News = $"You expected there to be news here? It's {DateTime.Now.Year}!";

        AnalyticsId = 0;
        AnalyticsEnabled = false;
        AnalyticsApiKey = "ANALYTICS_KEY";
        BaseUrl = "http://localhost";

        CrashOnError = false;
        LogAssets = true;
        DisableVersions = true;
        CacheLicense = "UNKNOWN";

        OverwriteGameConfig = true;
        ProjectName = "MQReawakened";
        HeaderFolderFilter = "_data";

        CacheVersion = 1;
        CacheSize = 0;
        CacheExpiration = 0;

        Is2014Client = true;
        TimeFilter = "yyyy-MM-dd_HH-mm-ss";
        OldClientLastUpdate = "2013-11-01_12-00-00";
        LastClientUpdate = DateTime.Now.ToUnixTimestamp();

        StartLauncherOnCommand = false;
    }
}
