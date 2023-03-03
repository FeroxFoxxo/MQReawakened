using A2m.Server;
using Server.Base.Network;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Extensions;

public static class NpcExtensions
{
    public static void AddQuest(this Player player, int questId, bool setActive, NetState state, QuestCatalog catalog)
    {
        var character = player.Character;
        var quest = catalog.GetQuestData(questId);

        if (quest == null || character == null)
            throw new InvalidDataException();

        var questModel = character.Data.QuestLog.FirstOrDefault(x => x.Id == questId) ??
                         new QuestStatusModel
                         {
                             QuestStatus = QuestStatus.QuestState.IN_PROCESSING,
                             Id = questId,
                             Objectives = quest.Objectives.ToDictionary(
                                 x => x.Key,
                                 x => new ObjectiveModel
                                 {
                                     Completed = false,
                                     CountLeft = x.Value.TotalCount
                                 }
                             )
                         };

        if (setActive)
            character.Data.ActiveQuestId = questId;

        state.SendXt("na", questModel, setActive);
    }
}
