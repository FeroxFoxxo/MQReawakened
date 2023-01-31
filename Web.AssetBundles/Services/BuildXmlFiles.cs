using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.Core.Abstractions;
using System.Text;
using System.Xml;
using Web.AssetBundles.Events;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Helpers;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Services;

public class BuildXmlFiles : IService, IInjectModules
{
    private readonly AssetBundleConfig _config;
    private readonly AssetEventSink _eventSink;
    private readonly ILogger<BuildXmlFiles> _logger;
    private readonly IServiceProvider _services;

    public Dictionary<string, string> XmlFiles;

    public BuildXmlFiles(AssetEventSink eventSink, IServiceProvider services, ILogger<BuildXmlFiles> logger,
        AssetBundleConfig config)
    {
        _eventSink = eventSink;
        _services = services;
        _logger = logger;
        _config = config;
    }

    public IEnumerable<Module> Modules { get; set; }

    public void Initialize() => _eventSink.AssetBundlesLoaded += LoadXmlFiles;

    private void LoadXmlFiles(AssetBundleLoadEventArgs assetLoadEvent)
    {
        _logger.LogInformation("Reading XML Files From Bundles");

        var assets = assetLoadEvent.InternalAssets
            .Select(x => x.Value)
            .Where(x => x.Type == AssetInfo.TypeAsset.XML)
            .ToArray();

        XmlFiles = new Dictionary<string, string>();

        Directory.CreateDirectory(_config.XmlSaveDirectory);

        using (var bar = new DefaultProgressBar(assets.Length, "Loading XML Files", _logger))
        {
            foreach (var asset in assets)
            {
                var text = WriteXmlToDisk(asset, bar);

                if (string.IsNullOrEmpty(text))
                {
                    bar.SetMessage($"XML for {asset.Name} is empty! Skipping...");
                    continue;
                }

                var path = Path.Join(_config.XmlSaveDirectory, $"{asset.Name}.xml");

                bar.SetMessage($"Writing file to {path}");

                File.WriteAllText(path, text);

                XmlFiles.Add(asset.Name, path);
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
