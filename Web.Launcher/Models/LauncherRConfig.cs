using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Enums;

namespace Web.Launcher.Models;

public class LauncherRConfig : IRConfig
{
    public ulong AnalyticsId { get; }
    public bool AnalyticsEnabled { get; }

    public string HeaderFolderFilter { get; }

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

    public string WinGameFolder { get; }
    public string WinLauncherFolder { get; }

    public string OSXGameFolder { get; }
    public string OSXLauncherFolder { get; }

    public LauncherRConfig()
    {
        AnalyticsId = 0;
        AnalyticsEnabled = true;

        CrashOnError = false;
        LogAssets = true;
        DisableVersions = true;
        CacheLicense = "UNKNOWN";

        OverwriteGameConfig = true;
        HeaderFolderFilter = "_data";

        CacheVersion = 1;
        CacheSize = 0;
        CacheExpiration = 0;

        TimeFilter = "yyyy-MM-dd_HH-mm-ss";

        ClientUpdates = new Dictionary<GameVersion, string>
        {
            { GameVersion.v2011, "2011-03-17_12-00-00" },
            { GameVersion.vEarly2012, "2012-01-01_12-00-00" },
            { GameVersion.vPets2012, "2012-05-27_12-00-00" },
            { GameVersion.vMinigames2012, "2012-08-01_12-00-00" },
            { GameVersion.vLate2012, "2012-10-01_12-00-00" },
            { GameVersion.vEarly2013, "2013-01-01_12-00-00" },
            { GameVersion.vLate2013, "2013-04-01_12-00-00" },
            { GameVersion.vEarly2014, "2013-11-22_12-00-00" },
            { GameVersion.vPetMasters2014, "2014-05-01_12-00-00" },
            { GameVersion.vLate2014, "2014-06-01_12-00-00" }
        };

        Fullscreen = false;
        OnGameClosePopup = false;

        WinGameFolder = InternalDirectory.GetDirectory("Game/Win");
        WinLauncherFolder = InternalDirectory.GetDirectory("Launcher/Win");

        OSXGameFolder = InternalDirectory.GetDirectory("Game/OSX");
        OSXLauncherFolder = InternalDirectory.GetDirectory("Launcher/OSX");
    }
}
