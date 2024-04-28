using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
public interface IQuestTriggered
{
    public void QuestAdded(QuestDescription quest, Player player);
    public void QuestCompleted(QuestDescription quest, Player player);
}
