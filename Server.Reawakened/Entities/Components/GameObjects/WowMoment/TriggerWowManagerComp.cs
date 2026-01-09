using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Entities.Components.GameObjects.WowMoment;
public class TriggerWowManagerComp : Component<TriggerWowManager>, ICoopTriggered
{
    public string CapeId => ComponentData.CapeId;
    public string SpiderId => ComponentData.SpiderId;
    public string DoorId => ComponentData.DoorId;
    public float QuestIDNeeded => ComponentData.QuestIDNeeded;
    public float PrevQuestID => ComponentData.PrevQuestID;

    public QuestCatalog QuestCatalog { get; set; }

    private BreakableEventControllerComp _spider;

    private BreakableEventControllerComp _spiderLeg;

    public override void DelayedComponentInitialization()
    {
        var trigger = Room.GetEntityFromId<TriggerCoopControllerComp>(Id);

        if (trigger is null)
            return;

        trigger.Triggers.TryAdd(Id, TriggerType.Activate);

        _spider = Room.GetEntityFromId<BreakableEventControllerComp>(SpiderId);

        _spiderLeg = Room.GetEntityFromId<BreakableEventControllerComp>(DoorId);

        if (_spider is null && _spiderLeg is null)
            return;

        TriggerStateChange(TriggerType.Activate, false, string.Empty);
    }

    public void TriggerStateChange(TriggerType triggerType, bool triggered, string triggeredBy)
    {
        if (_spider is null && _spiderLeg is null)
            return;

        _spider.CanBreak = triggered;
        _spiderLeg.CanBreak = triggered;
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player) { }
}
