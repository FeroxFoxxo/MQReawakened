using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Enums;

namespace Server.Reawakened.BundleHost.Configs;

public class AssetBundleRConfig : IRConfig
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
    public string CopiedCurrentBundles { get; }
    public string LocalAssetsDirectory { get; }
    public string ScriptsConfigDirectory { get; }

    public string SaveBundleExtension { get; }
    public string DefaultWebPlayerCacheLocation { get; }

    public bool AlwaysRecreateBundle { get; }
    public bool DebugInfo { get; }

    public string[] ForceLocalAsset { get; }
    public string[] AssetModifiers { get; }
    public Dictionary<GameVersion, Dictionary<string, string>> AssetRenames { get; }

    public bool LogAssetLoadInfo { get; }

    public AssetBundleRConfig()
    {
        AssetSaveDirectory = InternalDirectory.GetDirectory("Assets/AssetDictionaries");
        BundleSaveDirectory = InternalDirectory.GetDirectory("Assets/Bundles");
        RemovedDuplicateDirectory = InternalDirectory.GetDirectory("Assets/RemovedDuplicates");
        CopiedCurrentBundles = InternalDirectory.GetDirectory("Assets/CopiedCurrentBundles");
        ScriptsConfigDirectory = InternalDirectory.GetInternalDirectory("Assets/Scripts");

        LocalAssetsDirectory = InternalDirectory.GetInternalDirectory("Assets/LocalAssets");

        AlwaysRecreateBundle = false;
        DebugInfo = false;
        ShouldLogAssets = false;

        SaveBundleExtension = "bundleGen";
        StoredAssetDict = "StoredAssets.xml";

        Message = "Loading Asset Bundles";
        DefaultWebPlayerCacheLocation = "AppData/LocalLow/Unity/WebPlayer/Cache";
        AssetModifiers = ["_nomesh"];

        AssetRenames = new Dictionary<GameVersion, Dictionary<string, string>>
        {
            {
                GameVersion.v2011,
                new Dictionary<string, string>()
                {
                    { "FX_GiftBoxconfettis", "FX_GiftBoxConfettis" }
                }
            }
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

        VirtualGoods =
        [
            "ItemCatalog",
            "PetAbilities",
            "UserGiftMessage",
            "vendor_catalogs",
            "IconBank_VGMT",
            "IconBank_Pets"
        ];

        ForceLocalAsset =
        [
            "vendor_catalogs"
        ];

        LogAssetLoadInfo = false;
    }
}
