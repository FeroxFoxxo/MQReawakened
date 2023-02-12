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
    private readonly AssetEventSink _eventSink;
    private readonly ILogger<BuildXmlFiles> _logger;
    private readonly ServerConfig _sConfig;
    private readonly IServiceProvider _services;

    public readonly Dictionary<string, string> XmlFiles;

    public BuildXmlFiles(AssetEventSink eventSink, IServiceProvider services, ILogger<BuildXmlFiles> logger,
        AssetBundleConfig config, ServerConfig sConfig)
    {
        _eventSink = eventSink;
        _services = services;
        _logger = logger;
        _config = config;
        _sConfig = sConfig;

        XmlFiles = new Dictionary<string, string>();
    }

    public IEnumerable<Module> Modules { get; set; }

    public void Initialize() => _eventSink.AssetBundlesLoaded += LoadXmlFiles;

    private void LoadXmlFiles(AssetBundleLoadEventArgs assetLoadEvent)
    {
        _logger.LogInformation("Reading XML Files From Bundles");

        XmlFiles.Clear();

        var assets = assetLoadEvent.InternalAssets
            .Select(x => x.Value)
            .Where(x => x.Type is AssetInfo.TypeAsset.XML or AssetInfo.TypeAsset.Level)
            .ToArray();

        GetDirectory.OverwriteDirectory(_config.XmlSaveDirectory);
        GetDirectory.OverwriteDirectory(_sConfig.LevelSaveDirectory);

        var bundles = _services.GetRequiredServices<IBundledXml>(Modules).ToDictionary(x => x.BundleName, x => x);

        foreach (var bundle in bundles)
            bundle.Value.InitializeVariables();

        using (var bar = new DefaultProgressBar(assets.Length, "Loading XML Files", _logger, _config))
        {
            foreach (var asset in assets)
            {
                var text = GetXmlData(asset, bar);

                if (string.IsNullOrEmpty(text))
                {
                    bar.SetMessage($"XML for {asset.Name} is empty! Skipping...");
                    continue;
                }

                var fileName = $"{asset.Name}.xml";
                string directory;

                switch (asset.Type)
                {
                    case AssetInfo.TypeAsset.Level:
                        directory = _sConfig.LevelSaveDirectory;
                        break;
                    case AssetInfo.TypeAsset.XML:
                        directory = _config.XmlSaveDirectory;

                        if (bundles.TryGetValue(asset.Name, out var bundle))
                        {
                            if (bundle.GetType().IsAssignableTo(typeof(ILocalizationXml)))
                            {
                                var locXmlBundle = (ILocalizationXml)bundle;

                                var localizedAsset = assets.FirstOrDefault(x =>
                                    string.Equals(x.Name, locXmlBundle.LocalizationName,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                );

                                var localizedXml = GetXmlData(localizedAsset, bar);

                                locXmlBundle.ReadLocalization(localizedXml);
                            }

                            var xml = new XmlDocument();
                            xml.LoadXml(text);

                            bundle.EditXml(xml);

                            text = xml.WriteToString();

                            bundle.ReadXml(text);
                            bundle.FinalizeBundle();

                            bar.SetMessage($"Loaded {asset.Name} From Disk");
                            bundles.Remove(asset.Name);
                        }

                        break;
                    default:
                        bar.SetMessage($"Could not find a way of handling a {asset.Type} asset type. Skipping!");
                        continue;
                }

                var path = Path.Join(directory, fileName);

                bar.SetMessage($"Writing file to {path}");

                File.WriteAllText(path, text);

                XmlFiles.Add(asset.Name, path);

                bar.TickBar();
            }
        }

        if (bundles.Count > 0)
        {
            _logger.LogCritical(
                "Could not find XML bundle for {Bundles}, returning...",
                string.Join(" ,", bundles.Keys)
            );

            _logger.LogCritical("Possible XML files:");

            foreach (var foundAsset in assets)
                _logger.LogError("    {BundleName}", foundAsset.Name);
        }

        _logger.LogDebug("Read XML files");
    }

    private string GetXmlData(InternalAssetInfo asset, DefaultProgressBar bar)
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
