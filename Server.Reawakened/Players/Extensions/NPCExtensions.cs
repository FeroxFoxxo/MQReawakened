using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Reawakened.Entities.Components.GameObjects.Items;
using Server.Reawakened.Entities.Components.GameObjects.NPC;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Text;
using static A2m.Server.QuestStatus;
using static CollectibleController;

namespace Server.Reawakened.Players.Extensions;

public static class NpcExtensions
{
    public static QuestStatusModel AddQuest(this Player player, QuestDescription quest, InternalQuestItem questItem,
        ItemCatalog itemCatalog, FileLogger fileLogger, string identifier, Microsoft.Extensions.Logging.ILogger logger, bool setActive = true, bool daily = false)
    {
        var questTest = player.Character.QuestLog.FirstOrDefault(q => q.Id == quest.Id);

        if (questTest != null)
            return questTest;

        if (player.Character.CompletedQuests.Contains(quest.Id) && !daily)
            return null;

        if (setActive)
            player.Character.Write.ActiveQuestId = quest.Id;

        var questModel = player.Character.QuestLog.FirstOrDefault(x => x.Id == quest.Id);

        if (questModel == null)
        {
            questModel = new QuestStatusModel()
            {
                Id = quest.Id,
                QuestStatus = QuestState.NOT_START,
                CurrentOrder = quest.Objectives.Values.Count > 0 ? quest.Objectives.Values.Min(x => x.Order) : 1,
                Objectives = quest.Objectives.OrderBy(q => q.Value.Order).ToDictionary(q => q.Key, q => new ObjectiveModel()
                {
                    Completed = false,
                    CountLeft = q.Value.TotalCount,
                    Total = q.Value.TotalCount,
                    GameObjectId = q.Value.GoId,
                    GameObjectLevelId = q.Value.GoLevelId,
                    ItemId = (int)q.Value.GetField("_itemId"),
                    LevelId = q.Value.LevelId,
                    ObjectiveType = q.Value.Type,
                    Order = q.Value.Order,
                    MultiScorePrefabs = []
                })
            };

            SetMultiscoreObjectives(questModel, itemCatalog);

            foreach (var objective in questModel.Objectives)
            {
                if (objective.Value.ObjectiveType == ObjectiveEnum.IdolCollect)
                {
                    if (player.Character.CollectedIdols.TryGetValue(objective.Value.LevelId, out var idols))
                    {
                        objective.Value.CountLeft = objective.Value.Total - idols.Count;

                        if (objective.Value.CountLeft <= 0)
                            objective.Value.Completed = true;
                    }
                }
                else if (objective.Value.ObjectiveType == ObjectiveEnum.Inventorycheck)
                {
                    if (player.Character.Inventory.Items.TryGetValue(objective.Value.ItemId, out var item))
                    {
                        objective.Value.CountLeft = objective.Value.Total - item.Count;

                        if (objective.Value.CountLeft <= 0)
                            objective.Value.Completed = true;
                    }
                }
            }

            player.Character.QuestLog.Add(questModel);
        }

        logger.LogTrace("[{QuestName} ({QuestId})] [ADD QUEST] Added by {Name}", quest.Name, quest.Id, identifier);

        var rewardIds = (Dictionary<int, int>)quest.GetField("_rewardItemsIds");
        var unknownRewards = rewardIds.Where(x => !itemCatalog.Items.ContainsKey(x.Key));

        if (unknownRewards.Any())
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Quest name: '{quest.Name}', given by {identifier} in {player.Room?.LevelInfo?.InGameName})");

            foreach (var reward in unknownRewards)
                sb.AppendLine($"Reward Id {reward.Key}, Count {reward.Value}");

            fileLogger.WriteGenericLog<NPCController>("unknown-rewards", $"[Unknown Quest {quest.Id} Rewards]", sb.ToString(),
                LoggerType.Error);
        }

        questModel.QuestStatus = questModel.Objectives.Any(x => !x.Value.Completed) ?
            QuestState.IN_PROCESSING :
            QuestState.TO_BE_VALIDATED;

        player.SendXt("na", questModel, setActive ? 1 : 0);

        player.UpdateNpcsInLevel(quest);

        logger.LogInformation("[{QuestName} ({QuestId})] [QUEST STARTED]", quest.Name, questModel.Id);

        player.Character.Write.ActiveQuestId = questModel.Id;

        UpdateActiveObjectives(player, itemCatalog);

        if (questItem.QuestItemList.TryGetValue(quest.Id, out var itemList))
        {
            foreach (var itemModel in itemList)
            {
                var item = itemCatalog.GetItemFromId(itemModel.ItemId);

                if (item == null)
                    continue;

                player.AddItem(item, itemModel.Count, itemCatalog);
            }

            player.SendUpdatedInventory();
        }

        if (player.Room != null)
            foreach (var trigger in player.Room.GetEntitiesFromType<IQuestTriggered>())
                trigger.QuestAdded(quest, player);

        return questModel;
    }

    public static void UpdateActiveObjectives(Player player, ItemCatalog itemCatalog)
    {
        foreach (var questCollectible in player.Room?.GetEntitiesFromType<QuestCollectibleControllerComp>())
        {
            var item = itemCatalog.GetItemFromPrefabName(questCollectible.PrefabName);

            foreach (var objective in player.Character.QuestLog.SelectMany(x => x.Objectives.Values).Where
                (x => x.GameObjectId.ToString() == questCollectible.Id || item != null && x.ItemId == item.ItemId))
                questCollectible.UpdateActiveObjectives(player, CollectibleState.Active);
        }
    }

    private static void SetMultiscoreObjectives(QuestStatusModel questModel, ItemCatalog itemCatalog)
    {
        foreach (var objective in questModel.Objectives.Values)
        {
            if (objective.ObjectiveType != ObjectiveEnum.Scoremultiple)
                continue;

            foreach (var objective2 in questModel.Objectives.Values)
            {
                var itemDesc = itemCatalog.GetItemFromId(objective.ItemId);

                if (objective2.ObjectiveType == ObjectiveEnum.Scoremultiple && objective2.Order == objective.Order && itemDesc != null)
                    objective2.MultiScorePrefabs.Add(itemDesc.PrefabName);
            }
        }
    }

    public static void UpdateNpcsInLevel(this Player player, QuestStatusModel status, QuestCatalog questCatalog)
    {
        var quest = questCatalog.QuestCatalogs[status.Id];
        UpdateNpcsInLevel(player, quest);
    }

    public static void UpdateAllNpcsInLevel(this Player player)
    {
        if (player.Room == null)
            return;

        foreach (var npc in player.Room.GetEntitiesFromType<NPCControllerComp>())
            npc.SendNpcInfo(player);
    }

    public static void UpdateNpcsInLevel(this Player player, QuestDescription quest)
    {
        if (player.Room == null || quest == null)
            return;

        foreach (var npc in player.Room.GetEntitiesFromType<NPCControllerComp>()
                .Where(e =>
                    e.Id == quest.QuestGiverGoId.ToString() || e.Name == quest.QuestgGiverName ||
                    e.Id == quest.ValidatorGoId.ToString() || e.Name == quest.ValidatorName
                )
            )
            npc.SendNpcInfo(player);
    }
}
