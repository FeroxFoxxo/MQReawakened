using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Reawakened.BundleHost.Events;
using Server.Reawakened.BundleHost.Events.Arguments;
using Server.Reawakened.BundleHost.Extensions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.BundleHost.Services;

public class BuildXmlFiles(AssetEventSink eventSink, IServiceProvider services,
    ILogger<BuildXmlFiles> logger, ServerRConfig rConfig) : IService, IInjectModules
{
    public readonly Dictionary<string, string> XmlFiles = [];

    public IEnumerable<Module> Modules { get; set; }

    public void Initialize() => eventSink.AssetBundlesLoaded += LoadXmlFiles;

    private void LoadXmlFiles(AssetBundleLoadEventArgs assetLoadEvent)
    {
        logger.LogDebug("Reading XML files from bundles");

        XmlFiles.Clear();

        InternalDirectory.OverwriteDirectory(rConfig.XMLDirectory);

        var bundles = services.GetRequiredServices<IBundledXml>(Modules)
            .ToDictionary(x => x.BundleName, x => x);

        foreach (var bundle in bundles)
        {
            foreach (var property in bundle.Value.GetProperties())
            {
                var service = services.GetService(property.PropertyType);

                if (service != null)
                    property.SetValue(bundle.Value, service);
            }

            bundle.Value.InitializeVariables();
        }

        var assets = assetLoadEvent.InternalAssets
            .Select(x => x.Value)
            .Where(x => x.Type is AssetInfo.TypeAsset.XML)
            .OrderBy(x => x.Name)
            .OrderByDescending(x => bundles.TryGetValue(x.Name, out var bundle) ? (int)bundle.Priority : 0)
            .ToArray();

        var localisedXmls = new List<string>();

        foreach (var asset in assets)
        {
            var text = asset.GetXmlData();

            if (string.IsNullOrEmpty(text))
                continue;

            if (bundles.TryGetValue(asset.Name, out var bundle))
            {
                var time = DateTimeOffset.FromUnixTimeSeconds(asset.CacheTime);

                logger.LogTrace("Loading XML: {BundleName} {DateTime}", asset.Name, time.Date > DateTime.UnixEpoch ? $"({time.Date.ToShortDateString()})" : string.Empty);

                if (bundle is ILocalizationXml localizedXmlBundle)
                {
                    var localizedAsset = assets.FirstOrDefault(x =>
                        string.Equals(x.Name, localizedXmlBundle.LocalizationName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

                    var localizedXml = localizedAsset.GetXmlData();

                    var localXml = new XmlDocument();
                    localXml.LoadXml(localizedXml);

                    localizedXmlBundle.EditLocalization(localXml);

                    localizedXml = localXml.WriteToString();

                    localizedXmlBundle.ReadLocalization(localizedXml);

                    var locPath = Path.Join(rConfig.XMLDirectory, $"{localizedAsset.Name}.xml");

                    File.WriteAllText(locPath, localizedXml);

                    if (!localisedXmls.Contains(localizedAsset.Name))
                        localisedXmls.Add(localizedAsset.Name);

                    bundles.Remove(localizedAsset.Name);

                    XmlFiles.Add(localizedAsset.Name, locPath);
                }

                var settings = new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true };
                var xmlDocument = new XmlDocument();

                using var reader = XmlReader.Create(new StringReader(text), settings);

                xmlDocument.Load(reader);

                bundle.EditDescription(xmlDocument);

                if (bundle is InternalXml internalXml)
                {
                    internalXml.ReadDescription(xmlDocument);
                }
                else
                {
                    text = xmlDocument.WriteToString();
                    bundle.ReadDescription(text);
                }

                bundle.FinalizeBundle();

                bundles.Remove(asset.Name);
            }

            if (!localisedXmls.Contains(asset.Name))
            {
                var path = Path.Join(rConfig.XMLDirectory, $"{asset.Name}.xml");

                File.WriteAllText(path, text);

                XmlFiles.Add(asset.Name, path);
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

        foreach (var foundAsset in assets.Where(x => x.Type == AssetInfo.TypeAsset.XML).OrderBy(x => x.Name))
            logger.LogError("    {BundleName}", foundAsset.Name);

        logger.LogInformation("Read XML files");
    }
}
