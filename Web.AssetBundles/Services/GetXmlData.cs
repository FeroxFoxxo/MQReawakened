using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.Configs;
using Server.Reawakened.XMLs.Bundles;

namespace Web.AssetBundles.Services;

public class GetXmlData : IService
{
    private readonly BuildAssetList _assets;
    private readonly ServerStaticConfig _config;
    private readonly ItemCatalog _itemCatalog;
    private readonly BuildLevelFiles _levels;
    private readonly ILogger<GetXmlData> _logger;
    private readonly ServerConsole _serverConsole;
    private readonly WorldGraph _worldGraph;

    public GetXmlData(ServerConsole serverConsole, ILogger<GetXmlData> logger,
        ServerStaticConfig config, WorldGraph worldGraph, ItemCatalog itemCatalog,
        BuildAssetList assets, BuildLevelFiles levels)
    {
        _serverConsole = serverConsole;
        _logger = logger;
        _config = config;
        _worldGraph = worldGraph;
        _itemCatalog = itemCatalog;
        _assets = assets;
        _levels = levels;
    }

    public void Initialize()
    {
        _serverConsole.AddCommand(
            "listLevels",
            "Lists out all the levels in the world graph.",
            NetworkType.Both,
            PrintLevels
        );

        _serverConsole.AddCommand(
            "listItems",
            "Lists out all the items in the catalog.",
            NetworkType.Both,
            PrintItems
        );

        Directory.CreateDirectory(_config.DataDirectory);
    }

    private void PrintItems(string[] command)
    {
        var itemInformation = new Dictionary<int, string>();
        var items = (Dictionary<int, ItemDescription>)_itemCatalog.GetField<ItemHandler>("_itemDescriptionCache");

        var shouldSimplify = _logger.Ask(
            "Would you like to have a simplified item description?",
            true
        );

        var shouldFilter = _logger.Ask(
            "Would you like the items filtered to only the ones that you have cached?",
            true
        );

        foreach (var item in items)
        {
            if (shouldFilter)
                if (!_assets.InternalAssets.ContainsKey(item.Value.PrefabName))
                    continue;

            var filteredText =
                shouldSimplify ? string.Empty : $" -{string.Join(',', item.ToString().Split(',').Skip(2))}";
            itemInformation.Add(item.Key, $"{item.Value.ItemName} ({item.Value.PrefabName}){filteredText}");
        }

        PrintOrFile("Items", itemInformation, items.Count);
    }

    private void PrintLevels(string[] command)
    {
        var levelInformation = new Dictionary<int, string>();

        var shouldFilter = _logger.Ask(
            "Would you like the levels filtered to only the ones that you have available to visit?",
            true
        );

        var levels = (Dictionary<string, int>)_worldGraph.GetField<WorldGraphXML>("_levelNameToID");

        var lowercasedLevels = _levels.LevelFiles.Keys.Select(x => x.ToLower()).ToArray();

        foreach (var levelInfo in levels)
        {
            if (shouldFilter)
                if (!lowercasedLevels.Contains(levelInfo.Key))
                    continue;

            var name = _worldGraph.GetInfoLevel(levelInfo.Value).InGameName;

            levelInformation.Add(levelInfo.Value, $"{name} ({levelInfo.Key})");
        }

        PrintOrFile("Levels", levelInformation, levels.Count);
    }

    private void PrintOrFile(string fileName, Dictionary<int, string> information, int total)
    {
        var formattedInfo = information
            .OrderBy(info => info.Key)
            .Select(info => $"[{info.Key}] {info.Value}")
            .ToArray();

        var shouldPrint = _logger.Ask(
            $"Would you like to print out the {fileName} in the console, in addition to it being saved to a file?",
            true
        );

        if (shouldPrint)
        {
            _logger.LogInformation("{FileName}:", fileName);

            foreach (var info in formattedInfo)
                _logger.LogInformation("{Information}", info);
        }

        var path = Path.Combine(_config.DataDirectory, $"{fileName}.txt");
        File.WriteAllText(path, string.Join('\n', formattedInfo));
        _logger.LogDebug("{Count}/{Total} {Name} saved in: '{Path}'", formattedInfo.Length, total, fileName, path);
    }
}
