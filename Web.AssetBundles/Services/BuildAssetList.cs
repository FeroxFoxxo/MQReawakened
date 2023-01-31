using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Helpers.Internal;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using System.Xml;
using Web.AssetBundles.BundleFix.Header.Models;
using Web.AssetBundles.Enums;
using Web.AssetBundles.Events;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Helpers;
using Web.AssetBundles.LocalAssets;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Services;

public class BuildAssetList : IService
{
    private readonly AssetEventSink _assetSink;
    private readonly AssetBundleConfig _config;
    private readonly ServerConsole _console;
    private readonly ILogger<BuildAssetList> _logger;
    private readonly EventSink _sink;
    public readonly Dictionary<string, string> AssetDict;

    public readonly Dictionary<string, string> PublishConfigs;
    public string AssetDictLocation;

    public Dictionary<string, InternalAssetInfo> InternalAssets;

    public BuildAssetList(ILogger<BuildAssetList> logger, AssetBundleConfig config,
        EventSink sink,
        AssetEventSink assetSink, ServerConsole console)
    {
        _logger = logger;
        _config = config;
        _sink = sink;
        _assetSink = assetSink;
        _console = console;

        PublishConfigs = new Dictionary<string, string>();
        AssetDict = new Dictionary<string, string>();
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(new ConsoleCommand("setAssetsToDefault",
            "Force generates asset dictionary from default caches directory.",
            _ => GenerateDefaultAssetList(true)));

        _console.AddCommand(new ConsoleCommand("changeDefaultCacheDir",
            "Change the default cache directory and regenerate dictionary.",
            _ =>
            {
                _config.CacheInfoFile = TryGetCacheInfoFile(string.Empty, CacheType.Own);
                GenerateDefaultAssetList(true);
            }));

        _console.AddCommand(new ConsoleCommand("addCachesToDict",
            "Adds a cache directory to the current asset dictionary.",
            _ =>
            {
                var cacheDir = Path.GetDirectoryName(TryGetCacheInfoFile(string.Empty, CacheType.Own));
                var assets = GetAssetsFromCache(cacheDir).Where(a => !InternalAssets.ContainsKey(a.Key));
                foreach (var asset in assets)
                {
                    _logger.LogDebug("Loading new cache file {Name} ({Type})", asset.Key, asset.Value.Type);
                    InternalAssets.Add(asset.Key, asset.Value);
                }
            }));

        _console.AddCommand(new ConsoleCommand("clearCache", "Clears the Web Player cache manually.",
            _ => EmptyWebCacheDirectory()));

        _config.CacheInfoFile = TryGetCacheInfoFile(_config.CacheInfoFile, CacheType.Own);

        Directory.CreateDirectory(_config.AssetSaveDirectory);
        Directory.CreateDirectory(_config.BundleSaveDirectory);

        if (_config.FlushCacheOnStart)
        {
            Empty(_config.BundleSaveDirectory);

            var shouldDelete = _config.DefaultDelete;

            if (!shouldDelete)
                shouldDelete = _logger.Ask(
                    "You have 'FLUSH CACHE ON START' enabled, which may delete cached files from the original game, as they use the same directory. " +
                    "Please ensure, if this is your first time running this project, that there are not files already in this directory. " +
                    "These would otherwise be valuable.\n" +
                    $"Please note: The WEB PLAYER cache is found in your {_config.DefaultWebPlayerCacheLocation} folder. " +
                    "Please make an __info file in here if it does not exist already."
                );

            if (shouldDelete)
            {
                _config.WebPlayerInfoFile = TryGetCacheInfoFile(_config.WebPlayerInfoFile, CacheType.WebPlayer);

                if (_config.WebPlayerInfoFile != _config.CacheInfoFile)
                {
                    if (!_config.DefaultDelete && EmptyWebCacheDirectory())
                        if (_logger.Ask(
                                "It is recommended to clean your caches each time in debug mode. " +
                                "Do you want to set this as the default action?"
                            ))
                            _config.DefaultDelete = true;
                }
                else
                {
                    _logger.LogError("Web player cache and saved directory should not be the same! Skipping...");
                    _config.WebPlayerInfoFile = string.Empty;
                }
            }
        }

        AssetDictLocation = Path.Combine(_config.AssetSaveDirectory, _config.StoredAssetDict);

        GenerateDefaultAssetList(false);
    }

    public bool EmptyWebCacheDirectory()
    {
        _config.WebPlayerInfoFile = TryGetCacheInfoFile(_config.WebPlayerInfoFile, CacheType.WebPlayer);

        var isDifferent = _config.WebPlayerInfoFile != _config.CacheInfoFile;

        if (isDifferent)
            Empty(Path.GetDirectoryName(_config.WebPlayerInfoFile));

        return isDifferent;
    }

    public static void Empty(string path)
    {
        var directory = new DirectoryInfo(path);
        foreach (var file in directory.GetFiles()) file.Delete();
        foreach (var subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
    }

    private string TryGetCacheInfoFile(string defaultFile, CacheType cache)
    {
        var name = cache switch
        {
            CacheType.Own => "Original",
            CacheType.WebPlayer => $"Web Player '{_config.DefaultWebPlayerCacheLocation}'",
            _ => throw new ArgumentOutOfRangeException(nameof(cache), cache, null)
        };

        _logger.LogInformation("Getting The {Type} Cache Directory", name);

        try
        {
            defaultFile = SetFileValue.SetIfNotNull(defaultFile, $"Get the {name} '__info' Cache File",
                $"{name} Info File (__info)\0__info\0");
        }
        catch
        {
            // ignored
        }

        while (true)
        {

            name = name.ToLower();

            if (string.IsNullOrEmpty(defaultFile) || !defaultFile.EndsWith("__info"))
            {
                _logger.LogError("Please enter the absolute file path for the {Type} '__info' cache file.", name);
                defaultFile = Console.ReadLine() ?? string.Empty;
                continue;
            }

            break;
        }

        _logger.LogDebug("Got the {Type} cache directory: {Directory}", name, Path.GetDirectoryName(defaultFile));

        return defaultFile;
    }

    private void GenerateDefaultAssetList(bool forceGenerate)
    {
        _logger.LogInformation("Getting Asset Dictionary");

        var dictExists = File.Exists(AssetDictLocation);

        InternalAssets = new Dictionary<string, InternalAssetInfo>();

        InternalAssets = !dictExists || forceGenerate
            ? GetAssetsFromCache(Path.GetDirectoryName(_config.CacheInfoFile))
            : GetAssetsFromDictionary(File.ReadAllText(AssetDictLocation)).OrderAssets();

        InternalAssets.AddModifiedAssets(_config);
        InternalAssets.AddLocalXmlFiles(_logger, _config);

        _logger.LogDebug("Loaded {Count} assets to memory.", InternalAssets.Count);

        SaveStoredAssets(InternalAssets.Values, AssetDictLocation);

        foreach (var asset in InternalAssets.Values.Where(x => x.Type == AssetInfo.TypeAsset.Unknown))
            _logger.LogError("Could not find type for asset '{Name}' in '{File}'.", asset.Name, asset.Path);

        var vgmtAssets = InternalAssets.Where(x =>
                _config.VirtualGoods.Any(a => string.Equals(a, x.Key) || x.Key.StartsWith($"{a}Dict_")))
            .ToDictionary(x => x.Key, x => x.Value);

        if (!vgmtAssets.Any())
            _logger.LogError("Could not find any virtual good assets! " +
                             "Try adding them into the LocalAsset directory. " +
                             "The game will not run without these.");

        var gameAssets = InternalAssets.Where(x => !vgmtAssets.ContainsKey(x.Key))
            .Select(x => x.Value).ToList();

        PublishConfigs.Clear();
        AssetDict.Clear();

        AddPublishConfiguration(gameAssets, _config.PublishConfigKey);
        AddAssetDictionary(gameAssets, _config.PublishConfigKey);

        AddPublishConfiguration(vgmtAssets.Values, _config.PublishConfigVgmtKey);
        AddAssetDictionary(vgmtAssets.Values, _config.PublishConfigVgmtKey);

        _logger.LogDebug("Generated default dictionaries.");

        _assetSink.InvokeAssetBundlesLoaded(new AssetBundleLoadEventArgs(InternalAssets));
    }

    private Dictionary<string, InternalAssetInfo> GetAssetsFromCache(string directoryPath)
    {
        if (_config.ShouldLogAssets)
            Logger.Default = new AssetBundleLogger(_logger);

        var assets = new List<InternalAssetInfo>();

        var directories = directoryPath.GetLowestDirectories();

        var singleAssets = new Dictionary<string, InternalAssetInfo>();

        using var defaultBar = new DefaultProgressBar(directories.Count, _config.Message, _logger);

        foreach (var asset in directories.Select(directory => GetAssetBundle(directory, defaultBar)))
        {
            if (asset != null)
                assets.Add(asset);

            defaultBar.TickBar();
        }

        defaultBar.SetMessage($"Finished {_config.Message}");

        foreach (var newAsset in assets)
        {
            if (singleAssets.TryGetValue(newAsset.Name, out var value))
            {
                if (value.Type == newAsset.Type)
                {
                    var oldAssetVersion = new UnityVersion(value.UnityVersion).GetVersionInfo();
                    var newAssetVersion = new UnityVersion(newAsset.UnityVersion).GetVersionInfo();

                    if (oldAssetVersion < newAssetVersion || oldAssetVersion == newAssetVersion &&
                        value.BundleSize < newAsset.BundleSize)
                        singleAssets[newAsset.Name] = newAsset;
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
            else
            {
                singleAssets.Add(newAsset.Name, newAsset);
            }
        }

        return singleAssets.Values.OrderAssets();
    }

    private InternalAssetInfo GetAssetBundle(string folderName, DefaultProgressBar bar)
    {
        if (Directory.GetFiles(folderName).Length > 2)
        {
            bar.SetMessage($"Directory {folderName} has more than one cache item, skipping!");
            return null;
        }

        var manager = new AssetsManager();
        manager.LoadFolder(folderName);

        var assetFile = manager.assetsFileList.FirstOrDefault();

        if (assetFile == null)
        {
            bar.SetMessage($"Could not find asset in {folderName}, skipping!");
            return null;
        }

        var asset = new InternalAssetInfo
        {
            Name = assetFile.GetMainAssetName(),
            Path = assetFile.fullName,

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
                        $"{_config.Message} - found possible level '{asset.Name}' in {assetFile.fileName.Split('/').Last()}");
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
                    $"{_config.Message} - found possible XML '{asset.Name}' in {assetFile.fileName.Split('/').Last()}");

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
            bar.SetMessage($"{_config.Message} - WARNING: could not find type of asset {asset.Name}");

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
        dict.SetAttribute("name", _config.AssetDictKey);
        dict.SetAttribute("value", _config.AssetDictConfigs[key]);
        root.AppendChild(dict);

        document.AppendChild(root);

        var config = document.WriteToString();
        File.WriteAllText(Path.Combine(_config.AssetSaveDirectory, _config.PublishConfigs[key]), config);
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
        File.WriteAllText(Path.Combine(_config.AssetSaveDirectory, _config.AssetDictConfigs[key]), assetDict);
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
