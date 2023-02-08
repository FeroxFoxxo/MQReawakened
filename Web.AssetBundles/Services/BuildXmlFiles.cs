using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.Configs;
using Server.Reawakened.XMLs.Abstractions;
using System.Text;
using System.Xml;
using Web.AssetBundles.Events;
using Web.AssetBundles.Events.Arguments;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Services;

public class BuildXmlFiles : IService, IInjectModules
{
    private readonly AssetBundleConfig _config;
    private readonly ServerConfig _sConfig;
    private readonly AssetEventSink _eventSink;
    private readonly ILogger<BuildXmlFiles> _logger;
    private readonly IServiceProvider _services;

    public Dictionary<string, string> XmlFiles;

    public BuildXmlFiles(AssetEventSink eventSink, IServiceProvider services, ILogger<BuildXmlFiles> logger,
        AssetBundleConfig config, ServerConfig sConfig)
    {
        _eventSink = eventSink;
        _services = services;
        _logger = logger;
        _config = config;
        _sConfig = sConfig;
    }

    public IEnumerable<Module> Modules { get; set; }

    public void Initialize() => _eventSink.AssetBundlesLoaded += LoadXmlFiles;

    private void LoadXmlFiles(AssetBundleLoadEventArgs assetLoadEvent)
    {
        _logger.LogInformation("Reading XML Files From Bundles");

        var assets = assetLoadEvent.InternalAssets
            .Select(x => x.Value)
            .Where(x => x.Type is AssetInfo.TypeAsset.XML or AssetInfo.TypeAsset.Level)
            .ToArray();

        XmlFiles = new Dictionary<string, string>();

        GetDirectory.OverwriteDirectory(_config.XmlSaveDirectory);
        GetDirectory.OverwriteDirectory(_sConfig.LevelSaveDirectory);

        using (var bar = new DefaultProgressBar(assets.Length, "Loading XML Files", _logger, _config))
        {
            foreach (var asset in assets)
            {
                var text = WriteXmlToDisk(asset, bar);

                if (string.IsNullOrEmpty(text))
                {
                    bar.SetMessage($"XML for {asset.Name} is empty! Skipping...");
                    continue;
                }

                var directory = asset.Type switch
                {
                    AssetInfo.TypeAsset.XML => _config.XmlSaveDirectory,
                    AssetInfo.TypeAsset.Level => _sConfig.LevelSaveDirectory,
                    _ => throw new ArgumentOutOfRangeException()
                };

                var path = Path.Join(directory, $"{asset.Name}.xml");

                bar.SetMessage($"Writing file to {path}");

                File.WriteAllText(path, text);

                XmlFiles.Add(asset.Name, path);

                bar.TickBar();
            }
        }

        foreach (var xmlBundle in _services.GetRequiredServices<IBundledXml>(Modules))
        {
            var asset = XmlFiles.FirstOrDefault(x =>
                string.Equals(x.Key, xmlBundle.BundleName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(asset.Key))
            {
                _logger.LogCritical("Could not find XML bundle for {BundleName}, returning...", xmlBundle.BundleName);
                _logger.LogCritical("Possible XML files:");

                foreach (var foundAsset in assets)
                    _logger.LogError("    {BundleName}", foundAsset.Name);
            }
            else
            {
                xmlBundle.LoadBundle(File.ReadAllText(asset.Value));
                _logger.LogInformation("Read {XMLFile} From Disk", asset.Key);
            }
        }

        _logger.LogDebug("Read XML files");
    }

    private string WriteXmlToDisk(InternalAssetInfo asset, DefaultProgressBar bar)
    {
        try
        {
            var file = File.ReadAllText(asset.Path);

            if (file.FirstOrDefault() == '<')
                try
                {
                    new XmlDocument().LoadXml(file);
                    return file;
                }
                catch (XmlException)
                {
                }

            var manager = new AssetsManager();

            manager.LoadFiles(asset.Path);

            var textAsset = manager.assetsFileList.First().ObjectsDic.Values.GetText(asset.Name);

            var text = Encoding.UTF8.GetString(textAsset.m_Script);
            var length = text.Split('\n').Length;

            bar.SetMessage($"Read XML {asset.Name} for {length} lines.");

            return text;
        }
        catch (Exception e)
        {
            _logger.LogError("{Name} could not load! Exception: {ExceptionName}. Skipping...", asset.Name, e);
            return string.Empty;
        }
    }
}
