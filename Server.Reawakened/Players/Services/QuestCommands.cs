using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Services;
public class QuestCommands(ServerConsole serverConsole, EventSink sink,
    QuestCatalog questCatalog, ItemCatalog catalog, WorldGraph worldGraph, ILogger<Coords> logger) : IService
{
    public void Initialize() => sink.WorldLoad += Load;

    public void Load() => serverConsole.AddCommand(
            "printUnknownRewards",
            "Prints the unknown rewards of all quests to the console.",
            NetworkType.Server,
            PrintUnknownRewards);

    private void PrintUnknownRewards(string[] command)
    {
        var quests = questCatalog.QuestCatalogs.Values.Where(x =>
            questCatalog.QuestLineCatalogs.TryGetValue(x.QuestLineId, out var questLine) &&
            questLine.ShowInJournal
        );

        foreach (var quest in quests)
        {
            var rewardItems = (Dictionary<int, int>) quest.GetField("_rewardItemsIds");
            var rewardItemsToChoose = (Dictionary<int, int>)quest.GetField("_rewardItemsToChooseIds");

            foreach(var rewardItem in rewardItemsToChoose)
            {
                if (!rewardItems.ContainsKey(rewardItem.Key))
                    rewardItems.Add(rewardItem.Key, rewardItem.Value);
            }

            var rewards = rewardItems.ToDictionary(x => x.Key, x => catalog.Items.TryGetValue(x.Key, out var item) ? item : null);

            var unknownRewards = rewards.Where(x => x.Value == null).Select(x => x.Key);
            var knownRewards = rewards.Where(x => x.Value != null).Select(x => x.Value.ItemName);

            if (unknownRewards.Any())
                logger.LogError("Unknown quest reward ids: '{RewardIds}' for quest '{Quest}' ({QuestId}), " +
                    "given by '{QuestGiver}' in {QuestLocation}{KnownItems}",
                    string.Join(", ", unknownRewards), quest.Title, quest.Name, quest.QuestgGiverName, 
                    worldGraph.LevelNameFromID(quest.QuestGiverLevelId), knownRewards.Any() ? $" - known items: '{string.Join(", ", knownRewards)}'" : string.Empty);
        }    
    }
}
