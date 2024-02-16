﻿using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using System.Text;
using static A2m.Server.QuestStatus;
using static CollectibleController;

namespace Server.Reawakened.Players.Extensions;

public static class NpcExtensions
{
    public static QuestStatusModel AddQuest(this Player player, QuestDescription quest,
        Microsoft.Extensions.Logging.ILogger logger, ItemCatalog itemCatalog, FileLogger fileLogger, string identifier)
    {
        var character = player.Character;
        var questId = quest.Id;

        character.Data.ActiveQuestId = questId;

        var questModel = character.Data.QuestLog.FirstOrDefault(x => x.Id == questId);

        if (questModel == null)
        {
            questModel = new QuestStatusModel()
            {
                Id = questId,
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
            }

            character.Data.QuestLog.Add(questModel);
        }

        logger.LogTrace("[{QuestName} ({QuestId})] [ADD QUEST] Added by {Name}", quest.Name, quest.Id, identifier);

        var rewardIds = (Dictionary<int, int>)quest.GetField("_rewardItemsIds");
        var unknownRewards = rewardIds.Where(x => !itemCatalog.Items.ContainsKey(x.Key));

        if (unknownRewards.Any())
        {
            var sb = new StringBuilder();

            foreach (var reward in unknownRewards)
                sb.AppendLine($"Reward Id {reward.Key}, Count {reward.Value}");

            fileLogger.WriteGenericLog<NPCController>("unknown-rewards", $"[Unknown Quest {quest.Id} Rewards]", sb.ToString(),
                LoggerType.Error);
        }

        questModel.QuestStatus = questModel.Objectives.Any(x => !x.Value.Completed) ?
            QuestState.IN_PROCESSING :
            QuestState.TO_BE_VALIDATED;

        player.SendXt("na", questModel, true);

        player.UpdateNpcsInLevel(quest);

        logger.LogInformation("[{QuestName} ({QuestId})] [QUEST STARTED]", quest.Name, questModel.Id);

        player.Character.Data.ActiveQuestId = questModel.Id;

        UpdateActiveObjectives(player);

        return questModel;
    }

    public static void UpdateActiveObjectives(Player player)
    {
        foreach (var questCollectible in player.Room?.GetEntitiesFromType<QuestCollectibleControllerComp>())
        {
            var item = player.DatabaseContainer.ItemCatalog.GetItemFromPrefabName(questCollectible.PrefabName);

            foreach (var objective in player.Character.Data.QuestLog.SelectMany(x => x.Objectives.Values).Where
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

    public static void UpdateNpcsInLevel(this Player player, QuestStatusModel status)
    {
        var quests = player.DatabaseContainer.Quests;
        var quest = quests.QuestCatalogs[status.Id];
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
