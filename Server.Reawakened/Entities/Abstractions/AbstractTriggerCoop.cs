using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using System.Text;
using static TriggerCoopController;

namespace Server.Reawakened.Entities.Abstractions;

public abstract class AbstractTriggerCoop<T> : SyncedEntity<T> where T : TriggerCoopController
{
    public List<int> CurrentInteractors;

    public bool IsActive = true;
    public bool IsEnabled = true;

    public Dictionary<int, TriggerType> Triggers;
    public List<ActivationType> Activations;

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
    public InteractionType InteractType => EntityData.InteractType;

    public float ActivationTimeAfterFirstInteraction => EntityData.ActivationTimeAfterFirstInteraction;

    public ILogger<TriggerCoopController> Logger { get; set; }

    public FileLogger FileLogger { get; set; }

    public override void InitializeEntity()
    {
        IsEnabled = IsEnable;
        IsActive = false;

        CurrentInteractors = [];

        Triggers = [];
        Activations = [];

        AddToTriggers(
        [
            TargetLevelEditorId,
            Target02LevelEditorId,
            Target03LevelEditorId,
            Target04LevelEditorId,
            Target05LevelEditorId,
            Target06LevelEditorId,
            Target07LevelEditorId,
            Target08LevelEditorId
        ], TriggerType.Activate);

        AddToTriggers(
        [
            TargetToDeactivateLevelEditorId,
            Target02ToDeactivateLevelEditorId,
            Target03ToDeactivateLevelEditorId,
            Target04ToDeactivateLevelEditorId
        ], TriggerType.Deactivate);

        AddToTriggers(
        [
            Target01ToEnableLevelEditorId,
            Target02ToEnableLevelEditorId,
            Target03ToEnableLevelEditorId,
            Target04ToEnableLevelEditorId,
            Target05ToEnableLevelEditorId
        ], TriggerType.Enable);

        AddToTriggers(
        [
            Target01ToDisableLevelEditorId,
            Target02ToDisableLevelEditorId,
            Target03ToDisableLevelEditorId,
            Target04ToDisableLevelEditorId,
            Target05ToDisableLevelEditorId
        ], TriggerType.Disable);

        if (TriggerOnPressed) Activations.Add(ActivationType.Pressed);
        if (TriggerOnFireDamage) Activations.Add(ActivationType.FireDamage);
        if (TriggerOnEarthDamage) Activations.Add(ActivationType.EarthDamage);
        if (TriggerOnAirDamage) Activations.Add(ActivationType.AirDamage);
        if (TriggerOnIceDamage) Activations.Add(ActivationType.IceDamage);
        if (TriggerOnLightningDamage) Activations.Add(ActivationType.LightningDamage);
        if (TriggerOnNormalDamage) Activations.Add(ActivationType.NormalDamage);
        if (TriggerOnGrapplingHook) Activations.Add(ActivationType.NormalDamage);
        if (!string.IsNullOrEmpty(TriggeredByItemInInventory)) Activations.Add(ActivationType.ItemInInventory);

        RunTrigger(null);
    }

    public void AddToTriggers(List<int> triggers, TriggerType triggerType)
    {
        foreach (var trigger in triggers.Where(trigger => trigger > 0))
            Triggers.TryAdd(trigger, triggerType);
    }
    
    public override void SendDelayedData(Player player)
    {
        var trigger = new Trigger_SyncEvent(Id.ToString(), Room.Time, false,
            "now", IsActive);

        player.SendSyncEventToPlayer(trigger);
    }

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (!IsEnabled || syncEvent.Type != SyncEvent.EventType.Trigger)
            return;

        if (!TriggerOnPressed)
            return;

        var tEvent = new Trigger_SyncEvent(syncEvent);

        LogTriggerEvent(tEvent);

        var updated = false;

        if (tEvent.Activate && !CurrentInteractors.Contains(player.GameObjectId))
        {
            CurrentInteractors.Add(player.GameObjectId);
            updated = true;
        }
        else if (!tEvent.Activate && CurrentInteractors.Contains(player.GameObjectId))
        {
            CurrentInteractors.Remove(player.GameObjectId);
            updated = true;
        }

        if (updated)
            RunTrigger(player);

        LogTrigger();
    }

    public virtual void Triggered(Player player, bool isSuccess, bool isActive)
    {

    }

    public void RunTrigger (Player player)
    {
        if (!IsActive)
        {
            if (CurrentInteractors.Count < NbInteractionsNeeded ||
                NbInteractionsMatchesNbPlayers && CurrentInteractors.Count < Room.Players.Count)
                return;

            Trigger(player, true);

            if (DisabledAfterActivation)
                IsEnabled = false;
        }
        else
        {
            var triggerRecieverActivated = TriggerReceiverActivated();

            if (StayTriggeredOnReceiverActivated && triggerRecieverActivated)
                return;

            if (StayTriggeredOnUnpressed)
                return;

            Trigger(player, false);
        }
    }

    public void LogTriggerEvent(Trigger_SyncEvent tEvent)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Success: {tEvent.Success}");
        sb.AppendLine($"Triggered By: {tEvent.TriggeredByID}");
        sb.AppendLine($"Activate: {tEvent.Activate}");

        FileLogger.WriteGenericLog<TriggerCoopController>("triggered-coop", $"[Trigger Event]", sb.ToString(), LoggerType.Trace);
    }

    public void LogTrigger()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Active: {IsActive}");
        sb.AppendLine($"Enabled: {IsEnabled}");

        if (DisabledAfterActivation)
            sb.AppendLine($"Disabled After Activation : {DisabledAfterActivation}");

        if (NbInteractionsNeeded > 0)
            sb.AppendLine($"Interactions: {CurrentInteractors.Count}/{NbInteractionsNeeded}");

        if (NbInteractionsMatchesNbPlayers)
            sb.AppendLine($"Number Of Interactions Matches Players: {NbInteractionsMatchesNbPlayers}");

        if (Triggers.Count > 0)
            sb.AppendLine($"Triggers: {string.Join(", ",
                Triggers.Select(e => $"{e.Key} -> {Enum.GetName(e.Value)}")
            )}");

        if (ActiveDuration > 0)
            sb.AppendLine($"Activate Duration: {ActiveDuration}");

        if (Activations.Count > 0)
            sb.AppendLine($"Activation Types: {string.Join(", ",
                Activations.Select(e => Enum.GetName(e))
            )}");

        if (!string.IsNullOrEmpty(TriggeredByItemInInventory))
            sb.AppendLine($"Triggered By Item In Inventory: {TriggeredByItemInInventory}");

        if (StayTriggeredOnUnpressed)
            sb.AppendLine($"Stay Triggered On Unpressed : {StayTriggeredOnUnpressed}");

        if (StayTriggeredOnReceiverActivated)
            sb.AppendLine($"Stay Triggered Receiver Activated : {StayTriggeredOnReceiverActivated}");

        if (Flip)
            sb.AppendLine($"Flip : {Flip}");

        if (!string.IsNullOrEmpty(ActiveMessage))
            sb.AppendLine($"Active Message : {ActiveMessage}");

        if (SendActiveMessageToObjectId > 0)
            sb.AppendLine($"Send Active Message To Object Id : {SendActiveMessageToObjectId}");

        if (!string.IsNullOrEmpty(DeactiveMessage))
            sb.AppendLine($"Deactive Message : {DeactiveMessage}");

        if (!string.IsNullOrEmpty(TimerSound) && TimerSound != "PF_FX_Timer")
            sb.AppendLine($"Timer Sound : {TimerSound}");

        if (!string.IsNullOrEmpty(TimerEndSound) && TimerEndSound != "PF_FX_Timer_End")
            sb.AppendLine($"Timer End Sound : {TimerEndSound}");

        if (!string.IsNullOrEmpty(QuestCompletedRequired))
            sb.AppendLine($"Quest Completed Required : {QuestCompletedRequired}");

        if (!string.IsNullOrEmpty(QuestInProgressRequired))
            sb.AppendLine($"Quest In Progress Required : {QuestInProgressRequired}");

        if (TriggerRepeatDelay > 0)
            sb.AppendLine($"Repeat Delay: {TriggerRepeatDelay}");

        if (InteractType != InteractionType.None)
            sb.Append($"Interaction Type: {InteractType}");

        if (ActivationTimeAfterFirstInteraction > 0)
            sb.AppendLine($"Activation Time After First Interaction: {ActivationTimeAfterFirstInteraction}");

        FileLogger.WriteGenericLog<TriggerCoopController>("triggered-coop", $"[Trigger {Id}]", sb.ToString(), LoggerType.Trace);
    }

    public void Trigger(Player player, bool active)
    {
        IsActive = active;

        foreach (var trigger in Triggers)
        {
            if (Room.Entities.TryGetValue(trigger.Key, out var triggers))
                if (triggers.Count > 0)
                {
                    var canTriggerEntities = triggers.OfType<ITriggerable>().ToArray();

                    if (canTriggerEntities.Length != 0)
                        foreach (var triggerEntity in canTriggerEntities)
                            triggerEntity.TriggerStateChange(trigger.Value, Room, IsActive);

                    continue;
                }

            LogTriggerErrors(trigger.Key, trigger.Value);
        }

        if (player != null)
        {
            Room.SentEntityTriggered(Id, player, true, IsActive);
            Triggered(player, true, IsActive);
        }
    }

    public void LogTriggerErrors(int triggerId, TriggerType type)
    {
        var sb2 = new StringBuilder();

        sb2.AppendLine($"State: {type}")
            .AppendLine($"Entities: {Room.GetUnknownEntityTypes(triggerId)}");

        FileLogger.WriteGenericLog<TriggerCoopController>("triggered-errors", $"Trigger {Id}", sb2.ToString(),
            LoggerType.Error);
    }

    private bool TriggerReceiverActivated()
    {
        var receivers = Room.GetEntities<TriggerReceiverEntity>();

        var triggers = Triggers
            .Where(r => r.Value == TriggerType.Activate)
            .Where(trigger => receivers.ContainsKey(trigger.Key));

        return triggers.Any() &&
            triggers.Select(trigger => receivers[trigger.Key])
            .All(receiver => receiver.Activated);
    }
}
