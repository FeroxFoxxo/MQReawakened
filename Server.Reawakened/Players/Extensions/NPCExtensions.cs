using Server.Base.Core.Extensions;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using static A2m.Server.QuestStatus;

namespace Server.Reawakened.Players.Extensions;

public static class NpcExtensions
{
    public static void AddQuest(this Player player, QuestDescription quest, int questId, bool setActiveQuest)
    {
        var character = player.Character;

        if (quest != null && character != null)
        {
            var questModel = character.Data.QuestLog.FirstOrDefault(x => x.Id == questId);

            if (questModel == null)
            {
                questModel = new QuestStatusModel()
                {
                    Id = questId,
                    QuestStatus = QuestState.NOT_START,
                    CurrentOrder = quest.Objectives.Values.Count > 0 ? quest.Objectives.Values.Min(x => x.Order) : 1,
                    Objectives = quest.Objectives.ToDictionary(q => q.Key, q => new ObjectiveModel()
                    {
                        Completed = false,
                        CountLeft = q.Value.TotalCount,
                        GameObjectId = q.Value.GoId,
                        GameObjectLevelId = q.Value.GoLevelId,
                        ItemId = (int)q.Value.GetField("_itemId"),
                        LevelId = q.Value.LevelId,
                        ObjectiveType = q.Value.Type,
                        Order = q.Value.Order
                    })
                };
                  character.Data.QuestLog.Add(questModel);
            }

            player.SendXt("na", questModel, setActiveQuest ? 1 : 0);
        }

        if (setActiveQuest)
            character.Data.ActiveQuestId = questId;
    }

    public static void UpdateNpcsInLevel(this Player player, QuestStatusModel status, QuestCatalog quests)
    {
        var quest = quests.QuestCatalogs[status.Id];
        UpdateNpcsInLevel(player, quest);
    }

    public static void UpdateNpcsInLevel(this Player player)
    {
        foreach (var npc in GetNpcs(player))
            npc.SendNpcInfo(player.Character, player.NetState);
    }

    public static void UpdateNpcsInLevel(this Player player, QuestDescription quest)
    {
        if (quest != null)
            foreach (var npc in GetNpcs(player).Where(e => e.Id == quest.QuestGiverGoId || e.Id == quest.ValidatorGoId))
                npc.SendNpcInfo(player.Character, player.NetState);
    }

    public static List<NPCControllerComp> GetNpcs(Player player)
    {
        if (player.Room != null && player.Character != null)
            if (player.Room.Entities != null)
                return player.Room.Entities
                    .SelectMany(e => e.Value)
                    .Where(e => e is NPCControllerComp)
                    .Select(e => e as NPCControllerComp)
                    .ToList();

        return [];
    }
}
