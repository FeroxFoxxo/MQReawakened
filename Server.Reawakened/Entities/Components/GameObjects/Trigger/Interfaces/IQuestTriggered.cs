using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
public interface IQuestTriggered
{
    void QuestAdded(QuestDescription quest, Player player);
    void QuestCompleted(QuestDescription quest, Player player);
}
