using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Enums;

namespace Server.Reawakened.BundleHost.Models;

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
    public string XmlSaveDirectory { get; }
    public string LocalAssetsDirectory { get; }

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
        XmlSaveDirectory = InternalDirectory.GetDirectory("XMLs/XMLFiles");

        AssetSaveDirectory = InternalDirectory.GetDirectory("Assets/AssetDictionaries");
        BundleSaveDirectory = InternalDirectory.GetDirectory("Assets/Bundles");
        RemovedDuplicateDirectory = InternalDirectory.GetDirectory("Assets/RemovedDuplicates");
        LocalAssetsDirectory = InternalDirectory.GetDirectory("Assets/LocalAssets");

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
                    { "WelcomeGamePopup", "NotificationNoMailPopup" },
                    { "FX_GiftBoxconfettis", "FX_GiftBoxConfettis" }
                }
            },
            {
                GameVersion.v2014,
                new Dictionary<string, string>()
                {
                    { "FX_R01_ShardDestroyed_Silver", "FX_R01_Shard_Absorb" },
                    { "FX_R01_EnergyDust01", "FX_R01_ShardEnergyTravel" }
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
