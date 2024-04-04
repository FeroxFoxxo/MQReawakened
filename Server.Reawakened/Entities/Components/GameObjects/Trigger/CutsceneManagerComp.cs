using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger;

public class CutsceneManagerComp : BaseTriggerCoopController<CutsceneManager>
{
    public int[] TriggeredRewards;

    public bool StartOnLoad => ComponentData.StartOnLoad;
    public string ObjectToAnimateId => ComponentData.ObjectToAnimateId;
    public float DelayOnActivation => ComponentData.DelayOnActivation;
    public string ObjectToActivateId => ComponentData.ObjectToActivateId;
    public string ActivationMessage => ComponentData.ActivationMessage;
    public string ActivateSFX => ComponentData.ActivateSFX;
    public float FightMusicStartDelay => ComponentData.FightMusicStartDelay;
    public int TargetReward01LevelEditorID => ComponentData.TargetReward01LevelEditorID;
    public int TargetReward02LevelEditorID => ComponentData.TargetReward02LevelEditorID;
    public int TargetReward03LevelEditorID => ComponentData.TargetReward03LevelEditorID;
    public int TargetReward04LevelEditorID => ComponentData.TargetReward04LevelEditorID;
    public int RewardCameraID => ComponentData.RewardCameraID;
    public float EndMessageDelay => ComponentData.EndMessageDelay;
    public string EndMessage => ComponentData.EndMessage;
    public bool PlayBattleMusic => ComponentData.PlayBattleMusic;
    public bool ArenaHUD => ComponentData.ArenaHUD;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        TriggeredRewards = [
            TargetReward01LevelEditorID,
            TargetReward02LevelEditorID,
            TargetReward03LevelEditorID,
            TargetReward04LevelEditorID
        ];
    }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        foreach (var entity in TriggeredRewards)
            foreach (var trigger in Room.GetEntitiesFromId<TriggerReceiverComp>(entity.ToString()))
                trigger.Trigger(true);
    }
}
