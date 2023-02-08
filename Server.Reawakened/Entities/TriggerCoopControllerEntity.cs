using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using System;
using System.Reflection;

namespace Server.Reawakened.Entities;

public class TriggerCoopControllerEntity : SyncedEntity<TriggerCoopController>
{
    public bool Activated = false;

    //public HashSet<int> InteractingPlayers;

    private List<int> _targetIds;
    private List<int> _targetDeactivateIds;
    private List<int> _targetEnableIds;
    private List<int> _targetDisableIds;

    public override void InitializeEntity()
    {
        //InteractingPlayers = new HashSet<int>();

        _targetIds = new List<int>();
        _targetDeactivateIds = new List<int>();
        _targetEnableIds = new List<int>();
        _targetDisableIds = new List<int>();

        PopulateIds();
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        var currentTrigger = new Trigger_SyncEvent(syncEvent);

        if (!Activated && currentTrigger.Activate)
        {
            Activated = true;
            var activateTrigger = new Trigger_SyncEvent(Id.ToString(), Level.Time, true, player.PlayerId.ToString(), true);

            Level.SendSyncEvent(activateTrigger);
            netState.SendSyncEventToPlayer(activateTrigger);
        }

        foreach(var id in _targetIds)
        {
            var rec = new TriggerReceiver_SyncEvent(id.ToString(), Level.Time, player.PlayerId.ToString(), true, 1f);
            Level.SendSyncEvent(rec);
            netState.SendSyncEventToPlayer(rec);
        }

        foreach(var id in _targetDeactivateIds)
        {
            var rec = new TriggerReceiver_SyncEvent(id.ToString(), Level.Time, player.PlayerId.ToString(), false, 1f);
            Level.SendSyncEvent(rec);
            netState.SendSyncEventToPlayer(rec);
        }

        foreach(var id in _targetEnableIds)
        {
            var rec = new TriggerReceiver_SyncEvent(id.ToString(), Level.Time, player.PlayerId.ToString(), true, 1f);
            Level.SendSyncEvent(rec);
            netState.SendSyncEventToPlayer(rec);
        }

        foreach(var id in _targetDisableIds)
        {
            var rec = new TriggerReceiver_SyncEvent(id.ToString(), Level.Time, player.PlayerId.ToString(), false, 1f);
            Level.SendSyncEvent(rec);
            netState.SendSyncEventToPlayer(rec);
        }

        //Level.SendSyncEvent(trigEvent);
        //netState.SendSyncEventToPlayer(trigEvent);
    }

    private void PopulateIds()
    {
        if (EntityData.TargetLevelEditorID != 0) _targetIds.Add(EntityData.TargetLevelEditorID);
        if (EntityData.Target02LevelEditorID != 0) _targetIds.Add(EntityData.Target02LevelEditorID);
        if (EntityData.Target03LevelEditorID != 0) _targetIds.Add(EntityData.Target03LevelEditorID);
        if (EntityData.Target04LevelEditorID != 0) _targetIds.Add(EntityData.Target04LevelEditorID);
        if (EntityData.Target05LevelEditorID != 0) _targetIds.Add(EntityData.Target05LevelEditorID);
        if (EntityData.Target06LevelEditorID != 0) _targetIds.Add(EntityData.Target06LevelEditorID);
        if (EntityData.Target07LevelEditorID != 0) _targetIds.Add(EntityData.Target07LevelEditorID);
        if (EntityData.Target08LevelEditorID != 0) _targetIds.Add(EntityData.Target08LevelEditorID);
        
        if (EntityData.TargetToDeactivateLevelEditorID != 0) _targetDeactivateIds.Add(EntityData.TargetToDeactivateLevelEditorID);
        if (EntityData.Target02ToDeactivateLevelEditorID != 0) _targetDeactivateIds.Add(EntityData.Target02ToDeactivateLevelEditorID);
        if (EntityData.Target03ToDeactivateLevelEditorID != 0) _targetDeactivateIds.Add(EntityData.Target03ToDeactivateLevelEditorID);
        if (EntityData.Target04ToDeactivateLevelEditorID != 0) _targetDeactivateIds.Add(EntityData.Target04ToDeactivateLevelEditorID);

        if (EntityData.Target01ToEnableLevelEditorID != 0) _targetEnableIds.Add(EntityData.Target01ToEnableLevelEditorID);
        if (EntityData.Target02ToEnableLevelEditorID != 0) _targetEnableIds.Add(EntityData.Target02ToEnableLevelEditorID);
        if (EntityData.Target03ToEnableLevelEditorID != 0) _targetEnableIds.Add(EntityData.Target03ToEnableLevelEditorID);
        if (EntityData.Target04ToEnableLevelEditorID != 0) _targetEnableIds.Add(EntityData.Target04ToEnableLevelEditorID);
        if (EntityData.Target05ToEnableLevelEditorID != 0) _targetEnableIds.Add(EntityData.Target05ToEnableLevelEditorID);

        if (EntityData.Target01ToDisableLevelEditorID != 0) _targetDisableIds.Add(EntityData.Target01ToDisableLevelEditorID);
        if (EntityData.Target02ToDisableLevelEditorID != 0) _targetDisableIds.Add(EntityData.Target02ToDisableLevelEditorID);
        if (EntityData.Target03ToDisableLevelEditorID != 0) _targetDisableIds.Add(EntityData.Target03ToDisableLevelEditorID);
        if (EntityData.Target04ToDisableLevelEditorID != 0) _targetDisableIds.Add(EntityData.Target04ToDisableLevelEditorID);
        if (EntityData.Target05ToDisableLevelEditorID != 0) _targetDisableIds.Add(EntityData.Target05ToDisableLevelEditorID);
    }
}
