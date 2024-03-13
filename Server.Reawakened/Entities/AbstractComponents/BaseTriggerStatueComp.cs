
using Server.Reawakened.Entities.Enums;

namespace Server.Reawakened.Entities.AbstractComponents;

public abstract class BaseTriggerStatueComp<T> : BaseTriggerCoopController<T> where T : TriggerStatue
{
    public int[] TriggeredRewards;

    public int Target09LevelEditorId => ComponentData.Target09LevelEditorID;
    public int Target10LevelEditorId => ComponentData.Target10LevelEditorID;
    public int Target11LevelEditorId => ComponentData.Target11LevelEditorID;
    public int Target12LevelEditorId => ComponentData.Target12LevelEditorID;

    public int TargetReward01LevelEditorID => ComponentData.TargetReward01LevelEditorID;
    public int TargetReward02LevelEditorID => ComponentData.TargetReward02LevelEditorID;
    public int TargetReward03LevelEditorID => ComponentData.TargetReward03LevelEditorID;
    public int TargetReward04LevelEditorID => ComponentData.TargetReward04LevelEditorID;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        AddToTriggers(
        [
            Target09LevelEditorId,
            Target10LevelEditorId,
            Target11LevelEditorId,
            Target12LevelEditorId
        ], TriggerType.Activate);

        TriggeredRewards = [
            TargetReward01LevelEditorID,
            TargetReward02LevelEditorID,
            TargetReward03LevelEditorID,
            TargetReward04LevelEditorID
        ];
    }
}
