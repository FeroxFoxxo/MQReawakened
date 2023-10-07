using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;
using Web.AssetBundles.Events;
using Web.AssetBundles.Events.Arguments;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Services;

public class BuildXmlFiles(AssetEventSink eventSink, IServiceProvider services, ILogger<BuildXmlFiles> logger,
    AssetBundleRConfig rConfig, AssetBundleRwConfig rwConfig) : IService, IInjectModules
{
    public readonly Dictionary<string, string> XmlFiles = new();

    public IEnumerable<Module> Modules { get; set; }

    public void Initialize() => eventSink.AssetBundlesLoaded += LoadXmlFiles;

    private void LoadXmlFiles(AssetBundleLoadEventArgs assetLoadEvent)
    {
        logger.LogDebug("Reading XML files from bundles");

        XmlFiles.Clear();

        var assets = assetLoadEvent.InternalAssets
            .Select(x => x.Value)
            .Where(x => x.Type is AssetInfo.TypeAsset.XML)
            .OrderBy(x => x.Name)
            .ToArray();

        InternalDirectory.OverwriteDirectory(rConfig.XmlSaveDirectory);

        var bundles = services.GetRequiredServices<IBundledXml>(Modules)
            .ToDictionary(x => x.BundleName, x => x);

        foreach (var bundle in bundles)
            bundle.Value.InitializeVariables();

        using (var bar = new DefaultProgressBar(assets.Length, "Loading XML Files", logger, rwConfig))
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

                    bundle.EditDescription(xml, services);

                    text = xml.WriteToString();

                    bundle.ReadDescription(text);
                    bundle.FinalizeBundle();

                    bar.SetMessage($"Loaded {asset.Name} From Disk");
                    bundles.Remove(asset.Name);
                }

                var path = Path.Join(rConfig.XmlSaveDirectory, $"{asset.Name}.xml");

                bar.SetMessage($"Writing file to {path}");

                File.WriteAllText(path, text);

                XmlFiles.Add(asset.Name, path);

                bar.TickBar();
            }
        }

        if (bundles.Count <= 0)
            return;

        if (bundles.Keys.Any(b => assets.FirstOrDefault(a => a.Name == b && a.Type == AssetInfo.TypeAsset.XML) != null))
        {
            logger.LogCritical(
                "Your asset bundle cache seems to have moved! Please run 'changeCacheDir' and select the correct directory."
            );

            return;
        }

        logger.LogCritical(
            "Could not find XML bundle for {Bundles}, returning...",
            string.Join(", ", bundles.Keys)
        );

        logger.LogCritical("Possible XML files:");

        foreach (var foundAsset in assets.Where(x => x.Type == AssetInfo.TypeAsset.XML))
            logger.LogError("    {BundleName}", foundAsset.Name);

        logger.LogInformation("Read XML files");
    }
}
