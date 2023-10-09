using Server.Reawakened.Entities;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models.Character;
using static A2m.Server.QuestStatus;

namespace Server.Reawakened.Players.Extensions;

public static class NpcExtensions
{
    public static void AddQuest(this Player player, QuestDescription quest, bool setActiveQuest)
    {
        var character = player.Character;

        if (quest == null || character == null)
            throw new InvalidDataException();

        var questModel = character.Data.QuestLog.FirstOrDefault(x => x.Id == quest.Id);

        if (questModel == null)
        {
            questModel = new QuestStatusModel()
            {
                Id = quest.Id,
                QuestStatus = QuestState.NOT_START,
                Objectives = quest.Objectives.ToDictionary(q => q.Key, q => new ObjectiveModel()
                {
                    Completed = false,
                    CountLeft = q.Value.TotalCount
                })
            };
            character.Data.QuestLog.Add(questModel);
        }

        if (setActiveQuest)
            character.Data.ActiveQuestId = quest.Id;

        player.SendXt("na", questModel, setActiveQuest ? 1 : 0);
    }

    public static void UpdateNpcsInLevel(this Player player, QuestDescription quest)
    {
        if (player.Room != null && player.Character != null)
            foreach (var entity in player.Room.Entities.SelectMany(e => e.Value)
                .Where(e => e.Id == quest.QuestGiverGoId || e.Id == quest.ValidatorGoId)
                .Where(e => e is NpcControllerEntity).Select(e => e as NpcControllerEntity))
                entity.SendNpcInfo(player.Character, player.NetState);
    }
}
