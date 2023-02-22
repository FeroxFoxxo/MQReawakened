using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.AssetBundles.Models;

public class AssetBundleStaticConfig : IStaticConfig
{
    public string AssetDictKey { get; }
    public string StoredAssetDict { get; }

    public bool ShouldLogAssets { get; }
    public string Message { get; }

    public string PublishConfigKey { get; }
    public string PublishConfigVgmtKey { get; }

    public Dictionary<string, string> PublishConfigs { get; }
    public Dictionary<string, string> AssetDictConfigs { get; }
    public string[] VirtualGoods { get; }

    public string BundleSaveDirectory { get; }
    public string AssetSaveDirectory { get; }
    public string RemovedDuplicateDirectory { get; }
    public string XmlSaveDirectory { get; }

    public string SaveBundleExtension { get; }
    public string DefaultWebPlayerCacheLocation { get; }

    public bool AlwaysRecreateBundle { get; }
    public bool DebugInfo { get; }

    public string[] ForceLocalAsset { get; }
    public string[] AssetModifiers { get; }
    public Dictionary<string, string> AssetRenames { get; }

    public bool UseCacheReplacementScheme { get; }

    public string LocalAssetCache { get; }
    public bool LogProgressBars { get; }
    public bool KillOnBundleRetry { get; }

    public AssetBundleStaticConfig()
    {
        BundleSaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "Bundles");
        XmlSaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "XMLs");
        AssetSaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "AssetDictionaries");
        RemovedDuplicateDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "RemovedDuplicates");
        LocalAssetCache = Path.Combine(InternalDirectory.GetBaseDirectory(), "TestAssetCache.cache");

        UseCacheReplacementScheme = true;
        LogProgressBars = false;

        AlwaysRecreateBundle = true;
        DebugInfo = true;
        ShouldLogAssets = false;
        KillOnBundleRetry = true;

        SaveBundleExtension = "bundleGen";
        StoredAssetDict = "StoredAssets.xml";

        Message = "Loading Asset Bundles";
        DefaultWebPlayerCacheLocation = "AppData/LocalLow/Unity/WebPlayer/Cache";
        AssetModifiers = new[] { "_nomesh" };

        AssetRenames = new Dictionary<string, string>
        {
            // UNKNOWN
            { "WelcomeGamePopup", "NotificationNoMailPopup" },
            { "FX_GiftBoxconfettis", "FX_GiftBoxConfettis" }
        };

        PublishConfigKey = "unity.game.publishconfig";
        PublishConfigVgmtKey = "unity.game.vgmt.publishconfig";

        AssetDictKey = "publish.asset_dictionary";

        PublishConfigs = new Dictionary<string, string>
        {
            { PublishConfigKey, "PublishConfiguration.xml" },
            { PublishConfigVgmtKey, "PublishConfiguration.VGMT.xml" }
        };

        AssetDictConfigs = new Dictionary<string, string>
        {
            { PublishConfigKey, "assetDictionary.xml" },
            { PublishConfigVgmtKey, "assetDictionary.VGMT.xml" }
        };

        VirtualGoods = new[]
        {
            "ItemCatalog",
            "PetAbilities",
            "UserGiftMessage",
            "vendor_catalogs",
            "IconBank_VGMT",
            "IconBank_Pets"
        };

        ForceLocalAsset = new[]
        {
            "vendor_catalogs"
        };
    }
}
