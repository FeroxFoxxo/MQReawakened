using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using System.Xml;
using Web.AssetBundles.Events;
using Web.AssetBundles.Events.Arguments;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Helpers;
using Web.AssetBundles.LocalAssets;
using Web.AssetBundles.Models;
using Web.Launcher.Models;

namespace Web.AssetBundles.Services;

public class BuildAssetList : IService
{
    private readonly AssetEventSink _assetSink;
    private readonly AssetBundleConfig _config;
    private readonly AssetBundleStaticConfig _sConfig;
    private readonly ServerConsole _console;
    private readonly StartConfig _startConfig;
    private readonly ILogger<BuildAssetList> _logger;
    private readonly EventSink _sink;

    public readonly Dictionary<string, string> AssetDict;

    public readonly List<string> CurrentlyLoadedAssets;
    public readonly Dictionary<string, string> PublishConfigs;

    public string AssetDictLocation;

    public Dictionary<string, InternalAssetInfo> InternalAssets;

    public BuildAssetList(ILogger<BuildAssetList> logger, AssetBundleStaticConfig sConfig,
        EventSink sink, AssetEventSink assetSink, ServerConsole console, StartConfig startConfig,
        AssetBundleConfig config)
    {
        _logger = logger;
        _sConfig = sConfig;
        _sink = sink;
        _assetSink = assetSink;
        _console = console;
        _startConfig = startConfig;
        _config = config;

        PublishConfigs = new Dictionary<string, string>();
        AssetDict = new Dictionary<string, string>();

        CurrentlyLoadedAssets = new List<string>();
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(new ConsoleCommand("refreshCacheDir",
            "Force generates asset dictionary from default caches directory.",
            _ => GenerateDefaultAssetList(true)));

        _console.AddCommand(new ConsoleCommand("changeCacheDir",
            "Change the default cache directory and regenerate dictionary.",
            _ =>
            {
                _config.CacheInfoFile = GetInfoFile.TryGetInfoFile("Original", string.Empty, _logger);
                GenerateDefaultAssetList(true);
            }));

        _config.CacheInfoFile = GetInfoFile.TryGetInfoFile("Original", _config.CacheInfoFile, _logger);

        Directory.CreateDirectory(_sConfig.AssetSaveDirectory);
        Directory.CreateDirectory(_sConfig.BundleSaveDirectory);

        if (_config.FlushCacheOnStart)
            GetDirectory.Empty(_sConfig.BundleSaveDirectory);

        AssetDictLocation = Path.Combine(_sConfig.AssetSaveDirectory, _sConfig.StoredAssetDict);

        GenerateDefaultAssetList(false);
    }

    private void GenerateDefaultAssetList(bool forceGenerate)
    {
        _logger.LogInformation("Getting Asset Dictionary");

        var dictExists = File.Exists(AssetDictLocation);

        var assets = !dictExists || forceGenerate
            ? GetAssetsFromCache(Path.GetDirectoryName(_config.CacheInfoFile))
            : GetAssetsFromDictionary(File.ReadAllText(AssetDictLocation));

        InternalAssets = assets.GetClosestBundles(_startConfig);

        InternalAssets.AddModifiedAssets(_sConfig);
        InternalAssets.AddLocalXmlFiles(_logger, _sConfig);

        _logger.LogDebug("Loaded {Count} assets to memory.", InternalAssets.Count);

        foreach (var asset in InternalAssets.Values.Where(x => x.Type == AssetInfo.TypeAsset.Unknown))
            _logger.LogError("Could not find type for asset '{Name}' in '{File}'.", asset.Name, asset.Path);

        var vgmtAssets = InternalAssets.Where(x =>
                _sConfig.VirtualGoods.Any(a => string.Equals(a, x.Key) || x.Key.StartsWith($"{a}Dict_")))
            .ToDictionary(x => x.Key, x => x.Value);

        if (!vgmtAssets.Any())
            _logger.LogError("Could not find any virtual good assets! " +
                             "Try adding them into the LocalAsset directory. " +
                             "The game will not run without these.");

        var gameAssets = InternalAssets.Where(x => !vgmtAssets.ContainsKey(x.Key))
            .Select(x => x.Value).ToList();

        PublishConfigs.Clear();
        AssetDict.Clear();

        AddPublishConfiguration(gameAssets, _sConfig.PublishConfigKey);
        AddAssetDictionary(gameAssets, _sConfig.PublishConfigKey);

        AddPublishConfiguration(vgmtAssets.Values, _sConfig.PublishConfigVgmtKey);
        AddAssetDictionary(vgmtAssets.Values, _sConfig.PublishConfigVgmtKey);

        _logger.LogDebug("Generated default dictionaries.");

        _assetSink.InvokeAssetBundlesLoaded(new AssetBundleLoadEventArgs(InternalAssets));
    }

    private IEnumerable<InternalAssetInfo> GetAssetsFromCache(string directoryPath)
    {
        if (_sConfig.ShouldLogAssets)
            Logger.Default = new AssetBundleLogger(_logger);

        var assets = new List<InternalAssetInfo>();

        var directories = directoryPath.GetLowestDirectories();

        using var defaultBar = new DefaultProgressBar(directories.Count, _sConfig.Message, _logger, _sConfig);

        foreach (var asset in directories.Select(directory => GetAssetBundle(directory, defaultBar)))
        {
            if (asset != null)
                assets.Add(asset);

            defaultBar.TickBar();
        }

        defaultBar.SetMessage($"Finished {_sConfig.Message}");

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

        var asset = new InternalAssetInfo
        {
            Name = assetFile.GetMainAssetName(bar),
            Path = assetFile.fullName,
            CacheTime = time,
            Version = 0,
            Type = AssetInfo.TypeAsset.Unknown,
            BundleSize = Convert.ToInt32(new FileInfo(assetFile.fullName).Length / 1024),
            Locale = RFC1766Locales.LanguageCodes.en_us,
            UnityVersion = assetFile.unityVersion
        };

        var gameObj = assetFile.ObjectsDic.Values.ToList().GetGameObject(asset.Name)?.m_Name;
        var musicObj = assetFile.ObjectsDic.Values.GetMusic(asset.Name)?.m_Name;
        var textObj = assetFile.ObjectsDic.Values.GetText(asset.Name)?.m_Name;

        if (!string.IsNullOrEmpty(gameObj))
        {
            asset.Name = gameObj;

            if (asset.Name.StartsWith("LV"))
                if (!asset.Name.Contains("mesh") && !asset.Name.Contains("plane"))
                {
                    asset.Type = AssetInfo.TypeAsset.Level;
                    bar.SetMessage(
                        $"{_sConfig.Message} - found possible level '{asset.Name}' in {assetFile.fileName.Split('/').Last()}");
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

            if (asset.Name.StartsWith("NavMesh"))
            {
                asset.Type = AssetInfo.TypeAsset.NavMesh;
            }
            else
            {
                bar.SetMessage(
                    $"{_sConfig.Message} - found possible XML '{asset.Name}' in {assetFile.fileName.Split('/').Last()}");

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
            bar.SetMessage($"{_sConfig.Message} - WARNING: could not find type of asset {asset.Name}");

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
        dict.SetAttribute("name", _sConfig.AssetDictKey);
        dict.SetAttribute("value", _sConfig.AssetDictConfigs[key]);
        root.AppendChild(dict);

        document.AppendChild(root);

        var config = document.WriteToString();
        File.WriteAllText(Path.Combine(_sConfig.AssetSaveDirectory, _sConfig.PublishConfigs[key]), config);
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
        File.WriteAllText(Path.Combine(_sConfig.AssetSaveDirectory, _sConfig.AssetDictConfigs[key]), assetDict);
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

    private static IEnumerable<InternalAssetInfo> GetAssetsFromDictionary(string xml)
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
