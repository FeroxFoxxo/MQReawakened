using Server.Base.Core.Extensions;
using Server.Reawakened.Entities;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using static A2m.Server.QuestStatus;

namespace Server.Reawakened.Players.Extensions;

public static class NpcExtensions
{
    public static void AddQuest(this Player player, QuestDescription quest, bool setActiveQuest)
    {
        var character = player.Character;

        if (quest == null || character == null)
            return;

        var questModel = character.Data.QuestLog.FirstOrDefault(x => x.Id == quest.Id);

        if (questModel == null)
        {
            questModel = new QuestStatusModel()
            {
                Id = quest.Id,
                QuestStatus = QuestState.NOT_START,
                CurrentOrder = quest.Objectives.Values.Count > 0 ? quest.Objectives.Values.Min(x => x.Order) : 1,
                Objectives = quest.Objectives.ToDictionary(q => q.Key, q => new ObjectiveModel()
                {
                    Completed = false,
                    CountLeft = q.Value.TotalCount,
                    GameObjectId = q.Value.GoId,
                    GameObjectLevelId = q.Value.GoLevelId,
                    ItemId = (int) q.Value.GetField("_itemId"),
                    LevelId = q.Value.LevelId,
                    ObjectiveType = q.Value.Type,
                    Order = q.Value.Order
                })
            };
            character.Data.QuestLog.Add(questModel);
        }

        if (setActiveQuest)
            character.Data.ActiveQuestId = quest.Id;

        player.SendXt("na", questModel, setActiveQuest ? 1 : 0);
    }

    public static void UpdateNpcsInLevel(this Player player, QuestStatusModel status, QuestCatalog quests)
    {
        var quest = quests.QuestCatalogs[status.Id];
        UpdateNpcsInLevel(player, quest);
    }

    public static void UpdateNpcsInLevel(this Player player, QuestDescription quest)
    {
        if (player.Room != null && player.Character != null && quest != null)
            if (player.Room.Entities != null)
                foreach (var entity in player.Room.Entities.SelectMany(e => e.Value)
                    .Where(e => e.Id == quest.QuestGiverGoId || e.Id == quest.ValidatorGoId)
                    .Where(e => e is NpcControllerEntity).Select(e => e as NpcControllerEntity))
                    entity.SendNpcInfo(player.Character, player.NetState);
    }
}
