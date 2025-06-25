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
using Server.Reawakened.BundleHost.Helpers;
using Server.Reawakened.BundleHost.Models;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Icons.Services;
using System.Xml;

namespace Server.Reawakened.BundleHost.Services;

public class BuildAssetList(ILogger<BuildAssetList> logger, EventSink sink, AssetEventSink assetSink, ServerConsole console,
    AssetBundleRwConfig rwConfig, AssetBundleRConfig rConfig, ServerRConfig sRConfig, ExtractIcons iconExtract) : IService
{
    public readonly Dictionary<string, string> AssetDict = [];

    public readonly Dictionary<string, string> PublishConfigs = [];

    public string AssetDictLocation;

    public Dictionary<string, InternalAssetInfo> InternalAssets;

    public void Initialize() => sink.WorldLoad += Load;

    public void Load()
    {
        console.AddCommand(
            "refreshCacheDir",
            "Force generates asset dictionary from default caches directory.",
            NetworkType.Server | NetworkType.Client,
            _ => GenerateDefaultAssetList(true)
        );

        console.AddCommand(
            "changeCacheDir",
            "Change the default cache directory and regenerate dictionary.",
            NetworkType.Server | NetworkType.Client,
            _ =>
            {
                rwConfig.CacheInfoFile = GetInfoFile.TryGetInfoFile("Original", string.Empty, logger);
                GenerateDefaultAssetList(true);
            }
        );
    }

    public void LoadAssets()
    {
        rwConfig.CacheInfoFile = GetInfoFile.TryGetInfoFile("Original", rwConfig.CacheInfoFile, logger);

        if (!string.IsNullOrEmpty(rwConfig.WebPlayerInfoFile))
            rwConfig.WebPlayerInfoFile = rwConfig.GetWebPlayerInfoFile(rConfig, logger);

        if (rwConfig.FlushCacheOnStart)
            InternalDirectory.Empty(rConfig.BundleSaveDirectory);

        AssetDictLocation = Path.Combine(rConfig.AssetSaveDirectory, rConfig.StoredAssetDict);

        GenerateDefaultAssetList(false);
    }

    private void GenerateDefaultAssetList(bool forceGenerate)
    {
        logger.LogDebug("Getting asset dictionary");

        var dictExists = File.Exists(AssetDictLocation);

        var assets = !dictExists || forceGenerate
            ? GetAssetsFromCache(Path.GetDirectoryName(rwConfig.CacheInfoFile))
            : GetAssetsFromDictionary(File.ReadAllText(AssetDictLocation));

        InternalAssets = assets.GetClosestBundles(sRConfig);

        InternalAssets.AddModifiedAssets(rConfig, sRConfig);
        InternalAssets.AddLocalXmlFiles(logger, rConfig);

        logger.LogInformation("Loaded {Count} assets to memory.", InternalAssets.Count);

        foreach (var asset in InternalAssets.Values.Where(x => x.Type == AssetInfo.TypeAsset.Unknown))
            logger.LogError("Could not find type for asset '{Name}' in '{File}'.", asset.Name, asset.Path);

        var vgmtAssets = InternalAssets.Where(x =>
                rConfig.VirtualGoods.Any(a => string.Equals(a, x.Key) || x.Key.StartsWith($"{a}Dict_")))
            .ToDictionary(x => x.Key, x => x.Value);

        if (vgmtAssets.Count == 0)
            logger.LogError("Could not find any virtual good assets! " +
                             "Try adding them into the LocalAsset directory. " +
                             "The game will not run without these.");

        var gameAssets = InternalAssets
            .Where(x => !vgmtAssets.ContainsKey(x.Key))
            .Select(x => x.Value)
            .ToArray();

        PublishConfigs.Clear();
        AssetDict.Clear();

        AddPublishConfiguration(gameAssets, rConfig.PublishConfigKey);
        AddAssetDictionary(gameAssets, rConfig.PublishConfigKey);

        AddPublishConfiguration(vgmtAssets.Values, rConfig.PublishConfigVgmtKey);
        AddAssetDictionary(vgmtAssets.Values, rConfig.PublishConfigVgmtKey);

        logger.LogInformation("Generated default dictionaries.");

        sRConfig.LoadedAssets = [.. InternalAssets.Keys];
        iconExtract.ExtractAllIcons(InternalAssets);

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

    private List<InternalAssetInfo> GetAssetsFromCache(string directoryPath)
    {
        logger.LogInformation("Loading assets from cache, this may take a while!");

        if (rConfig.ShouldLogAssets)
            Logger.Default = new AssetBundleLogger(logger);

        var assets = new List<InternalAssetInfo>();

        var directories = directoryPath.GetLowestDirectories();

        using var defaultBar = new DefaultProgressBar(directories.Count, rConfig.Message, logger, rwConfig);

        foreach (var asset in directories.Select(directory => GetAssetBundle(directory, defaultBar)))
        {
            if (asset != null)
                assets.Add(asset);

            defaultBar.TickBar();
        }

        defaultBar.SetMessage($"Finished {rConfig.Message}");

        SaveStoredAssets(assets.OrderAssets(), AssetDictLocation);

        return assets;
    }

    private InternalAssetInfo GetAssetBundle(string folderName, DefaultProgressBar bar)
    {
        if (Directory.GetFiles(folderName).Length > 2)
        {
            bar.SetMessage($"Directory {folderName} has more than one cache item, skipping!");
            return null;
        }

        var infoFile = Path.Join(folderName, "__info");

        if (!File.Exists(infoFile))
        {
            bar.SetMessage($"Could not find info file in {folderName}, skipping!");
            return null;
        }

        var text = File.ReadAllLines(infoFile);

        if (text.Length < 4)
        {
            bar.SetMessage(
                $"Info file for {Path.GetDirectoryName(infoFile)} has only {text.Length} lines of text, skipping!");
            return null;
        }

        var time = long.Parse(text[1]);
        var file = Path.Join(folderName, text[3]);

        if (!File.Exists(file))
        {
            bar.SetMessage(
                $"Asset bundle for {Path.GetDirectoryName(file)} does not exist, skipping!!");
            return null;
        }

        var manager = new AssetsManager();
        manager.LoadFiles(file);

        var assetFile = manager.assetsFileList.FirstOrDefault();

        if (assetFile == null)
        {
            bar.SetMessage($"Could not find asset in {folderName}, skipping!");
            return null;
        }

        var name = assetFile.GetMainAssetName(bar);

        if (string.IsNullOrEmpty(name))
        {
            bar.SetMessage($"Could not find asset name in {folderName}, skipping!");
            return null;
        }

        var asset = new InternalAssetInfo
        {
            Name = name,
            Path = assetFile.fullName,
            CacheTime = time,
            Version = 0,
            Type = AssetInfo.TypeAsset.Unknown,
            BundleSize = Convert.ToInt32(new FileInfo(assetFile.fullName).Length / 1024),
            Locale = RFC1766Locales.LanguageCodes.en_us,
            UnityVersion = assetFile.unityVersion
        };

        var gameObj = assetFile.ObjectsDic.Values.ToArray().GetGameObject(asset.Name)?.m_Name;
        var musicObj = assetFile.ObjectsDic.Values.ToArray().GetMusic(asset.Name)?.m_Name;
        var textObj = assetFile.ObjectsDic.Values.ToArray().GetText(asset.Name)?.m_Name;

        if (!string.IsNullOrEmpty(gameObj))
        {
            asset.Name = gameObj;
            if (asset.Name.StartsWith("LV") || asset.Name.EndsWith("Level"))
                if (!asset.Name.Contains("mesh") && !asset.Name.Contains("plane"))
                {
                    asset.Type = AssetInfo.TypeAsset.Level;
                    bar.SetMessage(
                        $"{rConfig.Message} - found possible level '{asset.Name}' in {assetFile.fileName.Split('/').Last()}");
                }

            if (asset.Type == AssetInfo.TypeAsset.Unknown)
                asset.Type = AssetInfo.TypeAsset.Prefab;
        }
        else if (!string.IsNullOrEmpty(musicObj))
        {
            asset.Name = musicObj;
            asset.Type = AssetInfo.TypeAsset.Audio;
        }
        else if (!string.IsNullOrEmpty(textObj))
        {
            asset.Name = textObj;

            // Adding a game version check of vMinigames2012 or deleting this
            // allows early 2012 to load could be a missing cache issue
            // this requires the 'refreshCacheDir' command to be run each time
            // you want to go back to other versions bc NavMesh files will not be present
            if (asset.Name.StartsWith("NavMesh") && sRConfig.GameVersion >= GameVersion.vMinigames2012)
                asset.Type = AssetInfo.TypeAsset.NavMesh;
            else
            {
                bar.SetMessage(
                    $"{rConfig.Message} - found possible XML '{asset.Name}' in {assetFile.fileName.Split('/').Last()}");

                if (Enum.TryParse<RFC1766Locales.LanguageCodes>(
                        asset.Name.Split('_').Last().Replace('-', '_'),
                        true,
                        out var type)
                   )
                    asset.Locale = type;

                asset.Type = AssetInfo.TypeAsset.XML;
            }
        }

        if (asset.Type == AssetInfo.TypeAsset.Unknown)
            bar.SetMessage($"{rConfig.Message} - WARNING: could not find type of asset {asset.Name}");

        return asset;
    }

    private void AddPublishConfiguration(IEnumerable<InternalAssetInfo> assets, string key)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("PublishConfiguration");

        var xmlElements = document.CreateElement("xml_version");

        foreach (var asset in assets.Where(x => x.Type == AssetInfo.TypeAsset.XML))
            xmlElements.AppendChild(asset.ToAssetXml("item", document));

        root.AppendChild(xmlElements);

        var dict = document.CreateElement("item");
        dict.SetAttribute("name", rConfig.AssetDictKey);
        dict.SetAttribute("value", rConfig.AssetDictConfigs[key]);
        root.AppendChild(dict);

        document.AppendChild(root);

        var config = document.WriteToString();
        File.WriteAllText(Path.Combine(rConfig.AssetSaveDirectory, rConfig.PublishConfigs[key]), config);
        PublishConfigs.Add(key, config);
    }

    private void AddAssetDictionary(IEnumerable<InternalAssetInfo> assets, string key)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("assets");

        foreach (var asset in assets)
            root.AppendChild(asset.ToPubXml("asset", document));

        document.AppendChild(root);

        var assetDict = document.WriteToString();
        File.WriteAllText(Path.Combine(rConfig.AssetSaveDirectory, rConfig.AssetDictConfigs[key]), assetDict);
        AssetDict.Add(key, assetDict);
    }

    private static void SaveStoredAssets(IEnumerable<InternalAssetInfo> assets, string saveDir)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("assets");

        foreach (var asset in assets)
            root.AppendChild(asset.ToStoredXml("asset", document));

        document.AppendChild(root);

        File.WriteAllText(saveDir, document.WriteToString());
    }

    public static IEnumerable<InternalAssetInfo> GetAssetsFromDictionary(string xml)
    {
        var configuration = new List<InternalAssetInfo>();

        var document = new XmlDocument();
        document.LoadXml(xml);

        if (document.DocumentElement == null)
            return configuration;

        foreach (XmlNode node in document.DocumentElement.ChildNodes)
        {
            if (node is not XmlElement assetElement)
                continue;

            configuration.Add(assetElement.XmlToAsset());
        }

        return configuration;
    }
}
