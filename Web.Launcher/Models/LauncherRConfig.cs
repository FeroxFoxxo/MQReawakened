using Server.Base.Core.Abstractions;
using Server.Reawakened.Configs;

namespace Web.Launcher.Models;

public class LauncherRConfig : IRConfig
{
    public string News { get; }

    public ulong AnalyticsId { get; }
    public bool AnalyticsEnabled { get; }

    public string HeaderFolderFilter { get; }

    public string ProjectName { get; }

    public bool CrashOnError { get; }
    public bool LogAssets { get; }
    public bool DisableVersions { get; }
    public string CacheLicense { get; }

    public int CacheVersion { get; }
    public int CacheSize { get; }
    public int CacheExpiration { get; }

    public bool OverwriteGameConfig { get; }
    public string TimeFilter { get; }
    public Dictionary<GameVersion, string> ClientUpdates { get; }

    public bool Fullscreen { get; }
    public bool OnGameClosePopup { get; }

    public LauncherRConfig()
    {
        News = $"You expected there to be news here? It's {DateTime.Now.Year}!";

        AnalyticsId = 0;
        AnalyticsEnabled = true;

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

        TimeFilter = "yyyy-MM-dd_HH-mm-ss";

        // Likely occured between Nov 20 - Nov 24
        ClientUpdates = new Dictionary<GameVersion, string>
        {
            { GameVersion.v2013, "2009-01-01_12-00-00" },
            { GameVersion.v2014, "2013-11-22_12-00-00" }
        };

        Fullscreen = false;
        OnGameClosePopup = false;
    }
}
