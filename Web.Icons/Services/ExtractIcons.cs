using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Reawakened.XMLs.Bundles;
using System.Collections;
using System.Collections.Specialized;
using Web.AssetBundles.Events;
using Web.AssetBundles.Events.Arguments;
using Web.AssetBundles.Models;
using Web.Icons.Configs;

namespace Web.Icons.Services;

public class ExtractIcons(AssetEventSink sink, IconsRConfig rConfig, IconsRwConfig rwConfig, AssetBundleRwConfig aRwConfig,
    ILogger<ExtractIcons> logger, IServiceProvider services, ServerHandler serverHandler, ItemCatalog itemCatalog) : IService
{
    public void Initialize() => sink.AssetBundlesLoaded += ExtractAllIcons;

    private void ExtractAllIcons(AssetBundleLoadEventArgs bundleEvent)
    {
        var count = 0;
        var assets = new List<InternalAssetInfo>();

        foreach (var iconBankName in rConfig.IconBanks)
        {
            var iconBank = bundleEvent.InternalAssets.FirstOrDefault(x =>
                x.Key.Equals(iconBankName, StringComparison.CurrentCultureIgnoreCase)
            ).Value;

            if (iconBank != null)
            {
                assets.Add(iconBank);

                if (rwConfig.Configs.TryGetValue(iconBank.Name, out var time))
                {
                    if (time == iconBank.CacheTime)
                        continue;
                    else
                        rwConfig.Configs[iconBank.Name] = iconBank.CacheTime;
                }
                else
                {
                    rwConfig.Configs.Add(iconBank.Name, iconBank.CacheTime);
                }

                count++;
            }
        }

        if (count > 0)
        {
            Directory.CreateDirectory(rConfig.IconDirectory).Empty();
            Directory.CreateDirectory(rConfig.UnknownItemsDirectory).Empty();

            foreach (var asset in assets)
                ExtractIconsFrom(asset, bundleEvent);

            services.SaveConfigs(serverHandler.Modules, logger);

            var icons = Directory.GetFiles(rConfig.IconDirectory);

            foreach (var icon in icons)
            {
                var name = Path.GetFileNameWithoutExtension(icon);
                var nameWExten = Path.GetFileName(icon);

                if (itemCatalog.Items.Any(x => x.Value.PrefabName == name))
                    continue;

                File.Copy(icon, Path.Combine(rConfig.UnknownItemsDirectory, nameWExten));
            }
        }
    }

    public void ExtractIconsFrom(InternalAssetInfo asset, AssetBundleLoadEventArgs bundleEvent)
    {
        var manager = new AssetsManager();
        var assemblyLoader = new AssemblyLoader();

        manager.LoadFiles(asset.Path);

        var bank = manager.assetsFileList
            .First().ObjectsDic.Values
            .Where(x => x.type == ClassIDType.MonoBehaviour)
            .Select(x => x as MonoBehaviour)
            .FirstOrDefault();

        var type = bank.ToType();

        if (type == null)
        {
            var m_Type = bank.ConvertToTypeTree(assemblyLoader);
            type = bank.ToType(m_Type);
        }

        var icons = new List<OrderedDictionary>();

        foreach (DictionaryEntry entry in type)
            if ((string)entry.Key == "Icons")
                icons = ((List<object>)entry.Value)
                    .Select(x => (OrderedDictionary)x)
                .ToList();

        var textureCount = new Dictionary<string, int>();

        foreach (var entry in icons)
        {
            var name = string.Empty;
            var texturePath = -1;

            foreach (DictionaryEntry entry2 in entry)
            {
                if ((string)entry2.Key == "Name")
                    name = (string)entry2.Value;
                else if ((string)entry2.Key == "Texture")
                    texturePath = (int)((OrderedDictionary)entry2.Value)["m_PathID"];
            }

            textureCount.TryAdd(name, texturePath);
        }

        var textures = manager.assetsFileList
            .First().ObjectsDic.Values
            .Where(x => x.type == ClassIDType.Texture2D)
            .Select(x => x as Texture2D)
            .ToArray();

        using var defaultBar = new DefaultProgressBar(textures.Length, $"Extracting icons for {asset.Name}...", logger, aRwConfig);

        foreach (var texture in textures)
        {
            defaultBar.TickBar();

            var name = textureCount.FirstOrDefault(x => x.Value == texture.m_PathID).Key;

            if (string.IsNullOrEmpty(name))
            {
                logger.LogError("Unknown texture path: {PathId}", texture.m_PathID);
                continue;
            }

            var shouldBreak = false;

            foreach (var invalidStart in rConfig.IgnoreStarting)
                if (name.StartsWith(invalidStart))
                {
                    shouldBreak = true;
                    continue;
                }

            if (shouldBreak)
                continue;

            var path = Path.Join(rConfig.IconDirectory, $"{name}.png");

            using var stream = texture.ConvertToStream(ImageFormat.Png, true);

            if (stream == null)
                continue;

            File.WriteAllBytes(path, stream.ToArray());
        }
    }
}
