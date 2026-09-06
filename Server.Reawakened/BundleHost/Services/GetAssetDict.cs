using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.BundleHost.Events;
using Server.Reawakened.BundleHost.Events.Arguments;
using Server.Reawakened.BundleHost.Extensions;
using Server.Reawakened.BundleHost.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players.Helpers;
using System.Xml;

namespace Server.Reawakened.BundleHost.Services;

public class GetAssetDict(ILogger<GetAssetDict> logger, EventSink sink, AssetEventSink assetSink, ServerConsole console,
    AssetBundleRwConfig rwConfig, AssetBundleRConfig rConfig, ServerRConfig sRConfig) : IService
{
    public readonly Dictionary<string, string> AssetDict = [];

    public readonly Dictionary<string, string> PublishConfigs = [];

    public string AssetDictLocation;

    public Dictionary<string, InternalAssetInfo> InternalAssets;

    public void Initialize() => sink.WorldLoad += Load;

    public void Load()
    {
        if (rwConfig.UseCustomAssetLoader)
        {
            console.AddCommand(
                "refreshCacheDir",
                "Force generates asset dictionary from default caches directory.",
                NetworkType.Server | NetworkType.Client,
                _ => GenerateDefaultAssetList(true)
            );
        }
    }

    public void LoadAssets()
    {
        if (string.IsNullOrEmpty(rwConfig.DatabaseDirectory))
        {
            rwConfig.CacheInfoFile = GetInfoFile.TryGetInfoFile("Original", rwConfig.CacheInfoFile, logger, rwConfig);
            var isUnix = GetOsType.IsUnix();

            var path = isUnix ? rwConfig.CacheInfoFile.Split('/') : rwConfig.CacheInfoFile.Split('\\');
            path[^1] = string.Empty;

            // Bundles
            var sb = isUnix ? new SeparatedStringBuilder('/') : new SeparatedStringBuilder('\\');
            path[^2] = "Bundles";
            foreach (var item in path)
                sb.Append(item);
            rwConfig.DatabaseDirectory = sb.ToString();

            // XMLs
            sb = isUnix ? new SeparatedStringBuilder('/') : new SeparatedStringBuilder('\\');
            path[^2] = "XMLs";
            foreach (var item in path)
                sb.Append(item);
            rwConfig.DatabaseXMLDirectory = sb.ToString();

            // Levels
            sb = isUnix ? new SeparatedStringBuilder('/') : new SeparatedStringBuilder('\\');
            path[^2] = "Levels";
            foreach (var item in path)
                sb.Append(item);
            rwConfig.DatabaseLevelDirectory = sb.ToString();
        }

        if (!string.IsNullOrEmpty(rwConfig.WebPlayerInfoFile))
            rwConfig.WebPlayerInfoFile = rwConfig.GetWebPlayerInfoFile(rConfig, logger);

        if (rwConfig.FlushCacheOnStart)
            InternalDirectory.Empty(rConfig.BundleSaveDirectory);

        AssetDictLocation = Path.Combine(rwConfig.DatabaseDirectory, rConfig.AssetDictionaryName);

        GenerateDefaultAssetList(false);
    }

    private void GenerateDefaultAssetList(bool forceGenerate)
    {
        logger.LogDebug("Getting asset dictionary");

        var dictExists = File.Exists(AssetDictLocation);

        var assets = GetAssetsFromDictionary(rwConfig, File.ReadAllText(AssetDictLocation));

        InternalAssets = assets.GetClosestBundles(sRConfig);

        var vgmtAssets = InternalAssets.Where(x =>
                rConfig.VirtualGoods.Any(a => string.Equals(a, x.Key) || x.Key.StartsWith($"{a}Dict_")))
            .ToDictionary(x => x.Key, x => x.Value);

        var gameAssets = InternalAssets
            .Where(x => !vgmtAssets.ContainsKey(x.Key))
            .Select(x => x.Value)
            .ToArray();

        PublishConfigs.Clear();
        AssetDict.Clear();

        AddPublishConfiguration("PublishConfiguration.xml", rConfig.PublishConfigKey);
        AddAssetDictionary("assetDictionary.xml", rConfig.PublishConfigKey);

        AddPublishConfiguration("PublishConfiguration.VGMT.xml", rConfig.PublishConfigVgmtKey);
        AddAssetDictionary("assetDictionary.VGMT.xml", rConfig.PublishConfigVgmtKey);

        if (sRConfig.LastClientUpdate != rwConfig.LastDecompiledScriptUpdate || forceGenerate)
        {
            logger.LogInformation("Emptying script directory as lengths don't match.");

            Directory.Delete(rConfig.ScriptsConfigDirectory, true);
            Directory.CreateDirectory(rConfig.ScriptsConfigDirectory);

            logger.LogInformation("Loading fresh scripts.");

            using var defaultBar = new DefaultProgressBar(gameAssets.Length, rConfig.Message, logger, rwConfig);

            foreach (var asset in gameAssets)
            {
                var manager = new AssetsManager();
                manager.LoadFiles(asset.Path);

                var assetFile = manager.assetsFileList.FirstOrDefault();

                defaultBar.TickBar();

                if (assetFile == null)
                    continue;

                assetFile.GetScriptsFromBundle(asset.Name, rConfig);
            }

            logger.LogDebug("Finished loading scripts.");

            rwConfig.LastDecompiledScriptUpdate = sRConfig.LastClientUpdate;
        }
        else
        {
            logger.LogInformation("Scripts found.");
        }

        assetSink.InvokeAssetBundlesLoaded(new AssetBundleLoadEventArgs(InternalAssets));
    }

    private void AddPublishConfiguration(string fileName, string key)
    {
        var config = File.ReadAllText(Path.Combine(rwConfig.DatabaseDirectory, fileName));
        PublishConfigs.Add(key, config);
    }

    private void AddAssetDictionary(string fileName, string key)
    {
        var assetDict = File.ReadAllText(Path.Combine(rwConfig.DatabaseDirectory, fileName));
        AssetDict.Add(key, assetDict);
    }

    public static IEnumerable<InternalAssetInfo> GetAssetsFromDictionary(AssetBundleRwConfig config, string xml)
    {
        var configuration = new List<InternalAssetInfo>();

        var document = new XmlDocument();
        document.LoadXml(xml);

        foreach (var bundle in Directory.GetFiles(config.DatabaseXMLDirectory))
        {
            var info = new DirectoryInfo(bundle);
            configuration.Add(new InternalAssetInfo
            {
                Name = info.Name.Split('.')[0],
                Version = 5,
                Type = AssetInfo.TypeAsset.XML,
                Locale = RFC1766Locales.LanguageCodes.en_us,
                BundleSize = 0,
                Path = bundle,
                UnityVersion = string.Empty,
                CacheTime = 1,
            }
            );
        }

        foreach (var bundle in Directory.GetFiles(config.DatabaseDirectory))
        {
            var info = new DirectoryInfo(bundle);
            configuration.Add(new InternalAssetInfo
            {
                Name = info.Name.Split('.')[0],
                Version = 5,
                Type = AssetInfo.TypeAsset.Prefab,
                Locale = RFC1766Locales.LanguageCodes.en_us,
                BundleSize = 0,
                Path = bundle,
                UnityVersion = string.Empty,
                CacheTime = 1,
            }
            );
        }

        return configuration;
    }
}
