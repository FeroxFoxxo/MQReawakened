using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.BundleHost.Models;
using Server.Reawakened.Icons.Configs;
using Server.Reawakened.XMLs.Bundles.Base;
using System.Collections;
using System.Collections.Specialized;

namespace Server.Reawakened.Icons.Services;

public class ExtractIcons(IconsRConfig rConfig, IconsRwConfig rwConfig, AssetBundleRwConfig aRwConfig,
    ILogger<ExtractIcons> logger, IServiceProvider services, ServerHandler serverHandler)
{
    private string[] _knownIconNames = [];

    public bool HasIcon(string name) => _knownIconNames.Contains(name.ToUpper());

    public void ExtractAllIcons(Dictionary<string, InternalAssetInfo> internalAssets)
    {
        var outOfDateCache = 0;
        var assets = new List<InternalAssetInfo>();

        logger.LogInformation("Reading icons from file");

        foreach (var iconBankName in rConfig.IconBanks)
        {
            var iconBank = internalAssets.FirstOrDefault(x =>
                x.Key.Equals(iconBankName, StringComparison.CurrentCultureIgnoreCase)
            ).Value;

            if (iconBank != null)
            {
                assets.Add(iconBank);

                if (rwConfig.Configs.TryGetValue(iconBank.Name, out var time))
                    if (time == iconBank.CacheTime)
                        continue;
                    else
                        rwConfig.Configs[iconBank.Name] = iconBank.CacheTime;
                else
                    rwConfig.Configs.Add(iconBank.Name, iconBank.CacheTime);

                outOfDateCache++;
            }
        }

        var knownIcons = new Dictionary<string, Dictionary<string, Texture2D>>();

        foreach (var asset in assets)
            knownIcons.TryAdd(asset.Name, GetIcons(asset));

        if (outOfDateCache > 0)
        {
            Directory.CreateDirectory(rConfig.IconDirectory).Empty();

            foreach (var asset in knownIcons)
                ExtractIconsFrom(asset.Value, asset.Key);

            services.SaveConfigs(serverHandler.Modules, logger);
        }

        var iconNames = new List<string>();

        foreach (var icons in knownIcons.Values)
            foreach (var icon in icons.Keys)
            {
                var newIconName = icon.ToUpper();

                if (!iconNames.Contains(newIconName))
                    iconNames.Add(newIconName);
            }

        _knownIconNames = [.. iconNames.OrderBy(a => a)];

        logger.LogDebug("Read {Count} icons from file.", _knownIconNames.Length);
    }

    public void CheckDuplicatedIcons(ItemCatalog itemCatalog)
    {
        Directory.CreateDirectory(rConfig.UnknownItemsDirectory).Empty();

        foreach (var icon in Directory.GetFiles(rConfig.IconDirectory))
        {
            var name = Path.GetFileNameWithoutExtension(icon);
            var nameWExten = Path.GetFileName(icon);

            if (itemCatalog.Items.Any(x => x.Value.PrefabName == name))
                continue;

            File.Copy(icon, Path.Combine(rConfig.UnknownItemsDirectory, nameWExten));
        }
    }

    private Dictionary<string, Texture2D> GetIcons(InternalAssetInfo asset)
    {
        var manager = new AssetsManager();

        manager.LoadFiles(asset.Path);

        var bank = manager.assetsFileList
            .First().Objects
            .Where(x => x is MonoBehaviour)
            .Select(x => x as MonoBehaviour)
            .FirstOrDefault();

        var type = bank.ToType();

        if (type == null)
        {
            var m_Type = bank.ConvertToTypeTree(new AssemblyLoader());
            type = bank.ToType(m_Type);
        }

        var icons = new List<OrderedDictionary>();

        foreach (DictionaryEntry entry in type)
            if ((string)entry.Key == "Icons")
                icons = [.. ((List<object>)entry.Value).Select(x => (OrderedDictionary)x)];

        var texturePaths = new Dictionary<string, int>();

        foreach (var entry in icons)
        {
            var name = string.Empty;
            var texturePath = -1;

            foreach (DictionaryEntry entry2 in entry)
                if ((string)entry2.Key == "Name")
                    name = (string)entry2.Value;
                else if ((string)entry2.Key == "Texture")
                    texturePath = (int)((OrderedDictionary)entry2.Value)["m_PathID"];

            texturePaths.TryAdd(name, texturePath);
        }

        var textureDictionary = new Dictionary<string, Texture2D>();

        foreach (var texture in manager.assetsFileList
            .First().ObjectsDic.Values
            .Where(x => x.type == ClassIDType.Texture2D)
            .Select(x => x as Texture2D))
        {
            var name = texturePaths.FirstOrDefault(x => x.Value == texture.m_PathID).Key;

            if (string.IsNullOrEmpty(name))
            {
                logger.LogError("Unknown texture path: {PathId}", texture.m_PathID);
                continue;
            }

            textureDictionary.TryAdd(name, texture);
        }

        return textureDictionary;
    }

    private void ExtractIconsFrom(Dictionary<string, Texture2D> textures, string assetName)
    {
        using var defaultBar = new DefaultProgressBar(textures.Count, $"Extracting icons for {assetName}...", logger, aRwConfig);

        foreach (var kvp in textures)
        {
            var name = kvp.Key;
            var texture = kvp.Value;

            defaultBar.TickBar();

            var shouldSkip = false;

            foreach (var invalidStart in rConfig.IgnoreStarting)
                if (name.StartsWith(invalidStart))
                {
                    shouldSkip = true;
                    break;
                }

            if (shouldSkip)
                continue;

            var path = Path.Join(rConfig.IconDirectory, $"{name}.png");

            try
            {
                using var stream = texture.ConvertToStream(ImageFormat.Png, true);

                if (stream == null)
                    continue;

                File.WriteAllBytes(path, stream.ToArray());
            }
            catch (TypeInitializationException e)
            {
                defaultBar.Dispose();
                logger.LogError(e, "Texture DLL files did not initialise! This is a known bug for linux users.");
                return;
            }
            catch (IOException e)
            {
                logger.LogError(e, "Could not extract icon for path {Path}.", path);
            }
        }
    }
}
