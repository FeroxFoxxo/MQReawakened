using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.AssetBundles.Models;

public class AssetBundleConfig : IConfig
{
    public string AssetDictKey { get; set; }
    public string StoredAssetDict { get; set; }

    public bool ShouldLogAssets { get; set; }
    public string Message { get; set; }

    public string CacheInfoFile { get; set; }
    public string WebPlayerInfoFile { get; set; }

    public string PublishConfigKey { get; set; }
    public string PublishConfigVgmtKey { get; set; }

    public Dictionary<string, string> PublishConfigs { get; set; }
    public Dictionary<string, string> AssetDictConfigs { get; set; }
    public List<string> VirtualGoods { get; set; }

    public string BundleSaveDirectory { get; set; }
    public string AssetSaveDirectory { get; set; }
    public string XmlSaveDirectory { get; set; }

    public string SaveBundleExtension { get; set; }
    public string DefaultWebPlayerCacheLocation { get; set; }

    public bool AlwaysRecreateBundle { get; set; }
    public bool FlushCacheOnStart { get; set; }
    public bool DebugInfo { get; set; }
    public bool DefaultDelete { get; set; }
    public bool StartLauncherOnReplace { get; set; }

    public AssetBundleConfig()
    {
        BundleSaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "Bundles");
        XmlSaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "XMLs");
        AssetSaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "AssetDictionaries");

        AlwaysRecreateBundle = true;
        FlushCacheOnStart = true;
        DebugInfo = true;
        DefaultDelete = false;
        StartLauncherOnReplace = false;
        ShouldLogAssets = false;

        SaveBundleExtension = "bundleGen";
        StoredAssetDict = "StoredAssets.xml";

        CacheInfoFile = string.Empty;
        Message = "Loading Asset Bundles";
        DefaultWebPlayerCacheLocation = "AppData/LocalLow/Unity/WebPlayer/Cache";

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

        VirtualGoods = new List<string>
        {
            "ItemCatalog",
            "PetAbilities",
            "UserGiftMessage",
            "vendor_catalogs",
            "IconBank_VGMT",
            "IconBank_Pets"
        };
    }
}
