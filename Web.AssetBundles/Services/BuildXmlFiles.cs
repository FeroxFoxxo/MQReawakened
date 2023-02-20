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
    private readonly AssetBundleStaticConfig _config;
    private readonly AssetEventSink _eventSink;
    private readonly ILogger<BuildXmlFiles> _logger;
    private readonly IServiceProvider _services;

    public readonly Dictionary<string, string> XmlFiles;

    public BuildXmlFiles(AssetEventSink eventSink, IServiceProvider services, ILogger<BuildXmlFiles> logger,
        AssetBundleStaticConfig config)
    {
        _eventSink = eventSink;
        _services = services;
        _logger = logger;
        _config = config;

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
            .Where(x => x.Type is AssetInfo.TypeAsset.XML)
            .ToArray();

        GetDirectory.OverwriteDirectory(_config.XmlSaveDirectory);

        var bundles = _services.GetRequiredServices<IBundledXml>(Modules)
            .ToDictionary(x => x.BundleName, x => x);

        foreach (var bundle in bundles)
            bundle.Value.InitializeVariables();

        using (var bar = new DefaultProgressBar(assets.Length, "Loading XML Files", _logger, _config))
        {
            foreach (var asset in assets)
            {
                var text = asset.GetXmlData(bar);

                if (string.IsNullOrEmpty(text))
                {
                    bar.SetMessage($"XML for {asset.Name} is empty! Skipping...");
                    continue;
                }

                if (bundles.TryGetValue(asset.Name, out var bundle))
                {
                    if (bundle is ILocalizationXml localizedXmlBundle)
                    {
                        var localizedAsset = assets.FirstOrDefault(x =>
                            string.Equals(x.Name, localizedXmlBundle.LocalizationName,
                                StringComparison.OrdinalIgnoreCase
                            )
                        );

                        var localizedXml = localizedAsset.GetXmlData(bar);

                        localizedXmlBundle.ReadLocalization(localizedXml);
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

                var path = Path.Join(_config.XmlSaveDirectory, $"{asset.Name}.xml");

                bar.SetMessage($"Writing file to {path}");

                File.WriteAllText(path, text);

                XmlFiles.Add(asset.Name, path);

                bar.TickBar();
            }
        }

        if (bundles.Count <= 0)
            return;

        _logger.LogCritical(
            "Could not find XML bundle for {Bundles}, returning...",
            string.Join(", ", bundles.Keys)
        );

        _logger.LogCritical("Possible XML files:");

        foreach (var foundAsset in assets.Where(x => x.Type == AssetInfo.TypeAsset.XML))
            _logger.LogError("    {BundleName}", foundAsset.Name);

        _logger.LogDebug("Read XML files");
    }
}
