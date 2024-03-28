using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.Configs;
using Server.Reawakened.XMLs.Bundles;

namespace Web.AssetBundles.Services;

public class GetXmlData(ServerConsole serverConsole, ILogger<GetXmlData> logger,
    ServerRConfig config, WorldGraph worldGraph, ItemCatalog itemCatalog,
    BuildAssetList assets, BuildLevelFiles levelFiles) : IService
{
    public void Initialize()
    {
        serverConsole.AddCommand(
            "listLevels",
            "Lists out all the levels in the world graph.",
            NetworkType.Server | NetworkType.Client,
            PrintLevels
        );

        serverConsole.AddCommand(
            "listItems",
            "Lists out all the items in the catalog.",
            NetworkType.Server | NetworkType.Client,
            PrintItems
        );
    }

    private void PrintItems(string[] command)
    {
        var itemInformation = new Dictionary<int, string>();

        var shouldSimplify = logger.Ask(
            "Would you like to have a simplified item description?",
            true
        );

        var shouldFilter = !logger.Ask(
            "Would you like the items to also include prefabs not cached? (i.e. unfiltered)",
            true
        );

        foreach (var item in itemCatalog.Items)
        {
            if (shouldFilter)
                if (!assets.InternalAssets.ContainsKey(item.Value.PrefabName))
                    continue;

            var filteredText = shouldSimplify ? string.Empty : $" -{string.Join(',', item.ToString().Split(',').Skip(2))}";
            itemInformation.Add(item.Key, $"{item.Value.ItemName} ({item.Value.PrefabName}){filteredText}");
        }

        PrintOrFile("Items", itemInformation, itemCatalog.Items.Count);
    }

    private void PrintLevels(string[] command)
    {
        var levelInformation = new Dictionary<int, string>();

        var shouldFilter = logger.Ask(
            "Would you like the levels filtered to only the ones that you have available to visit?",
            true
        );

        var levels = (Dictionary<string, int>)worldGraph.GetField<WorldGraphXML>("_levelNameToID");

        var lowercasedLevels = levelFiles.LevelFiles.Keys.Select(x => x.ToLower()).ToArray();

        foreach (var levelInfo in levels)
        {
            if (shouldFilter)
                if (!lowercasedLevels.Contains(levelInfo.Key))
                    continue;

            var name = worldGraph.GetInfoLevel(levelInfo.Value).InGameName;

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

        var shouldPrint = logger.Ask(
            $"Would you like to print out the {fileName} in the console, in addition to it being saved to a file?",
            true
        );

        if (shouldPrint)
        {
            logger.LogInformation("{FileName}:", fileName);

            foreach (var info in formattedInfo)
                logger.LogInformation("{Information}", info);
        }

        var path = Path.Combine(config.DataDirectory, $"{fileName}.txt");
        File.WriteAllText(path, string.Join('\n', formattedInfo));
        logger.LogDebug("{Count}/{Total} {Name} saved in: '{Path}'", formattedInfo.Length, total, fileName, path);
    }
}
