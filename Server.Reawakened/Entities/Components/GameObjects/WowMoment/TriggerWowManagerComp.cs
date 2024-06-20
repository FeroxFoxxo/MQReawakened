using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Entities.Components.GameObjects.WowMoment;
public class TriggerWowManagerComp : Component<TriggerWowManager>, IQuestTriggered
{
    public string CapeId => ComponentData.CapeId;
    public string SpiderId => ComponentData.SpiderId;
    public string DoorId => ComponentData.DoorId;
    public float QuestIDNeeded => ComponentData.QuestIDNeeded;
    public float PrevQuestID => ComponentData.PrevQuestID;

    public QuestCatalog QuestCatalog { get; set; }

    private BreakableEventControllerComp _breakableController;

    public override void DelayedComponentInitialization()
    {
        var trigger = Room.GetEntityFromId<TriggerCoopControllerComp>(Id);

        if (trigger is null)
            return;

        trigger.Triggers.TryAdd(DoorId, TriggerType.Activate);

        _breakableController = Room.GetEntityFromId<BreakableEventControllerComp>(SpiderId);

        if (_breakableController is null)
            return;

        TriggerStateChange(false);
    }

    public void TriggerStateChange(bool triggered)
    {
        if (_breakableController is null)
            return;

        _breakableController.CanBreak = triggered;
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player) { }

    public void QuestAdded(QuestDescription quest, Player player)
    {
        var questId = Convert.ToInt32(PrevQuestID);

        if (quest.Id == QuestIDNeeded && player.Character.CompletedQuests.Contains(questId))
            TriggerStateChange(true);
    }

    public void QuestCompleted(QuestDescription quest, Player player)
    {
        var questId = Convert.ToInt32(PrevQuestID);

        if (quest.Id == PrevQuestID && player.Character.QuestLog.Any(q => q.Id == questId))
            TriggerStateChange(true);
    }
}
