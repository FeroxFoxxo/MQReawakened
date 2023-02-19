using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities.Abstractions;

public abstract class AbstractTriggerCoop<T> : SyncedEntity<T> where T : TriggerCoopController
{
    public bool DisabledAfterActivation => EntityData.DisabledAfterActivation;

    public int NbInteractionsNeeded => EntityData.NbInteractionsNeeded;
    public bool NbInteractionsMatchesNbPlayers => EntityData.NbInteractionsMatchesNbPlayers;

    public int TargetLevelEditorId => EntityData.TargetLevelEditorID;
    public int Target02LevelEditorId => EntityData.Target02LevelEditorID;
    public int Target03LevelEditorId => EntityData.Target03LevelEditorID;
    public int Target04LevelEditorId => EntityData.Target04LevelEditorID;
    public int Target05LevelEditorId => EntityData.Target05LevelEditorID;
    public int Target06LevelEditorId => EntityData.Target06LevelEditorID;
    public int Target07LevelEditorId => EntityData.Target07LevelEditorID;
    public int Target08LevelEditorId => EntityData.Target08LevelEditorID;

    public int TargetToDeactivateLevelEditorId => EntityData.TargetToDeactivateLevelEditorID;
    public int Target02ToDeactivateLevelEditorId => EntityData.Target02ToDeactivateLevelEditorID;
    public int Target03ToDeactivateLevelEditorId => EntityData.Target03ToDeactivateLevelEditorID;
    public int Target04ToDeactivateLevelEditorId => EntityData.Target04ToDeactivateLevelEditorID;

    public bool IsEnable => EntityData.isEnable;

    public int Target01ToEnableLevelEditorId => EntityData.Target01ToEnableLevelEditorID;
    public int Target02ToEnableLevelEditorId => EntityData.Target02ToEnableLevelEditorID;
    public int Target03ToEnableLevelEditorId => EntityData.Target03ToEnableLevelEditorID;
    public int Target04ToEnableLevelEditorId => EntityData.Target04ToEnableLevelEditorID;
    public int Target05ToEnableLevelEditorId => EntityData.Target05ToEnableLevelEditorID;

    public int Target01ToDisableLevelEditorId => EntityData.Target01ToDisableLevelEditorID;
    public int Target02ToDisableLevelEditorId => EntityData.Target02ToDisableLevelEditorID;
    public int Target03ToDisableLevelEditorId => EntityData.Target03ToDisableLevelEditorID;
    public int Target04ToDisableLevelEditorId => EntityData.Target04ToDisableLevelEditorID;
    public int Target05ToDisableLevelEditorId => EntityData.Target05ToDisableLevelEditorID;

    public float ActiveDuration => EntityData.ActiveDuration;

    public bool TriggerOnPressed => EntityData.TriggerOnPressed;
    public bool TriggerOnFireDamage => EntityData.TriggerOnFireDamage;
    public bool TriggerOnEarthDamage => EntityData.TriggerOnEarthDamage;
    public bool TriggerOnAirDamage => EntityData.TriggerOnAirDamage;
    public bool TriggerOnIceDamage => EntityData.TriggerOnIceDamage;
    public bool TriggerOnLightningDamage => EntityData.TriggerOnLightningDamage;
    public bool TriggerOnNormalDamage => EntityData.TriggerOnNormalDamage;

    public bool StayTriggeredOnUnpressed => EntityData.StayTriggeredOnUnpressed;
    public bool StayTriggeredOnReceiverActivated => EntityData.StayTriggeredOnReceiverActivated;

    public string TriggeredByItemInInventory => EntityData.TriggeredByItemInInventory;
    public bool TriggerOnGrapplingHook => EntityData.TriggerOnGrapplingHook;

    public bool Flip => EntityData.Flip;

    public string ActiveMessage => EntityData.ActiveMessage;
    public int SendActiveMessageToObjectId => EntityData.SendActiveMessageToObjectID;
    public string DeactiveMessage => EntityData.DeactiveMessage;

    public string TimerSound => EntityData.TimerSound;
    public string TimerEndSound => EntityData.TimerEndSound;

    public string QuestCompletedRequired => EntityData.QuestCompletedRequired;
    public string QuestInProgressRequired => EntityData.QuestInProgressRequired;

    public float TriggerRepeatDelay => EntityData.TriggerRepeatDelay;
    public TriggerCoopController.InteractionType InteractType => EntityData.InteractType;

    public float ActivationTimeAfterFirstInteraction => EntityData.ActivationTimeAfterFirstInteraction;

    public List<int> TargetIds;
    public List<int> TargetDeactivateIds;
    public List<int> TargetEnableIds;
    public List<int> TargetDisableIds;
    public bool Enabled;
    public List<int> CurrentInteractors;

    public override void InitializeEntity()
    {
        Enabled = IsEnable;

        CurrentInteractors = new List<int>();

        TargetIds = new List<int>
        {
            TargetLevelEditorId,
            Target02LevelEditorId,
            Target03LevelEditorId,
            Target04LevelEditorId,
            Target05LevelEditorId,
            Target06LevelEditorId,
            Target07LevelEditorId,
            Target08LevelEditorId
        };

        TargetDeactivateIds = new List<int>
        {
            TargetToDeactivateLevelEditorId,
            Target02ToDeactivateLevelEditorId,
            Target03ToDeactivateLevelEditorId,
            Target04ToDeactivateLevelEditorId
        };

        TargetEnableIds = new List<int>
        {
            Target01ToEnableLevelEditorId,
            Target02ToEnableLevelEditorId,
            Target03ToEnableLevelEditorId,
            Target04ToEnableLevelEditorId,
            Target05ToEnableLevelEditorId
        };

        TargetDisableIds = new List<int>
        {
            Target01ToDisableLevelEditorId,
            Target02ToDisableLevelEditorId,
            Target03ToDisableLevelEditorId,
            Target04ToDisableLevelEditorId,
            Target05ToDisableLevelEditorId
        };
    }

    public override void SendDelayedData(NetState netState) =>
        netState.SentEntityTriggered(Id, true, Enabled);

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var tEvent = new Trigger_SyncEvent(syncEvent);
        var player = netState.Get<Player>();

        var hasUpdated = false;

        if (tEvent.Activate)
        {
            if (!CurrentInteractors.Contains(player.PlayerId))
            {
                CurrentInteractors.Add(player.PlayerId);
                hasUpdated = true;
            }
        }
        else
        {
            if (CurrentInteractors.Contains(player.PlayerId))
            {
                CurrentInteractors.Remove(player.PlayerId);
                hasUpdated = true;
            }
        }

        if (!hasUpdated)
            return;
    }
}
