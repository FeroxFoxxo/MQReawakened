using A2m.Server;
using Server.Base.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using System.Text;
using UnityEngine;
using static TriggerCoopController;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;

public abstract class BaseTriggerCoopController<T> : Component<T>, ITriggerComp, IQuestTriggered where T : TriggerCoopController
{
    public bool DisabledAfterActivation => ComponentData.DisabledAfterActivation;

    public int NbInteractionsNeeded => ComponentData.NbInteractionsNeeded;
    public bool NbInteractionsMatchesNbPlayers => ComponentData.NbInteractionsMatchesNbPlayers;

    public int TargetLevelEditorId => ComponentData.TargetLevelEditorID;
    public int Target02LevelEditorId => ComponentData.Target02LevelEditorID;
    public int Target03LevelEditorId => ComponentData.Target03LevelEditorID;
    public int Target04LevelEditorId => ComponentData.Target04LevelEditorID;
    public int Target05LevelEditorId => ComponentData.Target05LevelEditorID;
    public int Target06LevelEditorId => ComponentData.Target06LevelEditorID;
    public int Target07LevelEditorId => ComponentData.Target07LevelEditorID;
    public int Target08LevelEditorId => ComponentData.Target08LevelEditorID;

    public int TargetToDeactivateLevelEditorId => ComponentData.TargetToDeactivateLevelEditorID;
    public int Target02ToDeactivateLevelEditorId => ComponentData.Target02ToDeactivateLevelEditorID;
    public int Target03ToDeactivateLevelEditorId => ComponentData.Target03ToDeactivateLevelEditorID;
    public int Target04ToDeactivateLevelEditorId => ComponentData.Target04ToDeactivateLevelEditorID;

    public bool IsEnable => ComponentData.isEnable;

    public int Target01ToEnableLevelEditorId => ComponentData.Target01ToEnableLevelEditorID;
    public int Target02ToEnableLevelEditorId => ComponentData.Target02ToEnableLevelEditorID;
    public int Target03ToEnableLevelEditorId => ComponentData.Target03ToEnableLevelEditorID;
    public int Target04ToEnableLevelEditorId => ComponentData.Target04ToEnableLevelEditorID;
    public int Target05ToEnableLevelEditorId => ComponentData.Target05ToEnableLevelEditorID;

    public int Target01ToDisableLevelEditorId => ComponentData.Target01ToDisableLevelEditorID;
    public int Target02ToDisableLevelEditorId => ComponentData.Target02ToDisableLevelEditorID;
    public int Target03ToDisableLevelEditorId => ComponentData.Target03ToDisableLevelEditorID;
    public int Target04ToDisableLevelEditorId => ComponentData.Target04ToDisableLevelEditorID;
    public int Target05ToDisableLevelEditorId => ComponentData.Target05ToDisableLevelEditorID;

    public float ActiveDuration => ComponentData.ActiveDuration;

    public bool TriggerOnPressed => ComponentData.TriggerOnPressed;
    public bool TriggerOnFireDamage => ComponentData.TriggerOnFireDamage;
    public bool TriggerOnEarthDamage => ComponentData.TriggerOnEarthDamage;
    public bool TriggerOnAirDamage => ComponentData.TriggerOnAirDamage;
    public bool TriggerOnIceDamage => ComponentData.TriggerOnIceDamage;
    public bool TriggerOnLightningDamage => ComponentData.TriggerOnLightningDamage;
    public bool TriggerOnNormalDamage => ComponentData.TriggerOnNormalDamage;

    public bool StayTriggeredOnUnpressed => ComponentData.StayTriggeredOnUnpressed;
    public bool StayTriggeredOnReceiverActivated => ComponentData.StayTriggeredOnReceiverActivated;

    public string TriggeredByItemInInventory => ComponentData.TriggeredByItemInInventory;
    public bool TriggerOnGrapplingHook => ComponentData.TriggerOnGrapplingHook;

    public bool Flip => ComponentData.Flip;

    public string ActiveMessage => ComponentData.ActiveMessage;
    public int SendActiveMessageToObjectId => ComponentData.SendActiveMessageToObjectID;
    public string DeactiveMessage => ComponentData.DeactiveMessage;

    public string TimerSound => ComponentData.TimerSound;
    public string TimerEndSound => ComponentData.TimerEndSound;

    public string QuestCompletedRequired => ComponentData.QuestCompletedRequired;
    public string QuestInProgressRequired => ComponentData.QuestInProgressRequired;

    public float TriggerRepeatDelay => ComponentData.TriggerRepeatDelay;
    public InteractionType InteractType => ComponentData.InteractType;

    public float ActivationTimeAfterFirstInteraction => ComponentData.ActivationTimeAfterFirstInteraction;

    public FileLogger FileLogger { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public List<string> CurrentPhysicalInteractors;
    public int CurrentInteractions;

    public List<Player> CurrentValidInteractors => [.. CurrentPhysicalInteractors.ToList().Select(ci =>
    {
        var player = Room.GetPlayerById(ci);

        var validQuestProgress = true;

        if (player == null && !ItemCatalog.GetItemFromId(int.Parse(ci)).IsPet() || ci == "0")
        {
            CurrentPhysicalInteractors.Remove(ci);
            return null;
        }

        if (!string.IsNullOrEmpty(QuestCompletedRequired))
        {
            var requiredQuest = QuestCatalog.QuestCatalogs.FirstOrDefault(q => q.Value.Name == QuestCompletedRequired).Value;

            if (requiredQuest != null)
                if (!player.Character.CompletedQuests.Contains(requiredQuest.Id))
                    validQuestProgress = false;
        }

        if (!string.IsNullOrEmpty(QuestInProgressRequired))
        {
            var requiredQuest = QuestCatalog.QuestCatalogs.FirstOrDefault(q => q.Value.Name == QuestInProgressRequired).Value;

            if (requiredQuest != null)
                if (player.Character.QuestLog.FirstOrDefault(q => q.Id == requiredQuest.Id) == null)
                    validQuestProgress = false;
        }

        return validQuestProgress ? player : null;

    }).Where(x => x != null)];

    public int Interactions => CurrentInteractions + CurrentValidInteractors.Count;

    public bool IsActive = false;
    public bool IsEnabled = false;
    public float LastActivationTime = 0;

    public Dictionary<string, TriggerType> Triggers;
    public List<ActivationType> Activations;

    public override void InitializeComponent()
    {
        IsEnabled = IsEnable;
        IsActive = false;

        CurrentPhysicalInteractors = [];

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

        if (TriggerOnNormalDamage || TriggerOnAirDamage || TriggerOnEarthDamage
            || TriggerOnFireDamage || TriggerOnIceDamage || TriggerOnLightningDamage)
        {
            var box = new Rect(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
            var position = new Vector3(Position.X, Position.Y, Position.Z);
            Room.AddCollider(new TriggerableTargetCollider(Id, position, box, ParentPlane, Room));
        }
    }

    public override void DelayedComponentInitialization() => RunTrigger(null);

    public void AddToTriggers(List<int> triggers, TriggerType triggerType)
    {
        foreach (var trigger in triggers.Where(trigger => trigger > 0))
            Triggers.TryAdd(trigger.ToString(), triggerType);
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

        var tEvent = new Trigger_SyncEvent(syncEvent);

        LogTriggerEvent(tEvent);

        var updated = false;

        if (tEvent.Activate && !CurrentPhysicalInteractors.Contains(player.GameObjectId))
        {
            AddPhysicalInteractor(player, player.GameObjectId);
            updated = true;
        }
        else if (!tEvent.Activate && CurrentPhysicalInteractors.Contains(player.GameObjectId))
        {
            RemovePhysicalInteractor(player, player.GameObjectId);
            updated = true;
        }

        if (updated)
            RunTrigger(player);
        else
            LogTrigger();
    }

    public void AddPhysicalInteractor(Player player, string interactionId)
    {
        if (CurrentPhysicalInteractors.Contains(interactionId)) return;

        CurrentPhysicalInteractors.Add(interactionId);

        if (Room.GetPlayerById(interactionId) != null)
        {
            var validQuestProgress = true;

            if (PlayerHasPet(player, out var pet))
                if (pet != null)
                    pet.CoopTriggerableId = Id;

            if (!string.IsNullOrEmpty(QuestCompletedRequired))
            {
                var requiredQuest = QuestCatalog.QuestCatalogs.FirstOrDefault(q => q.Value.Name == QuestCompletedRequired).Value;

                if (requiredQuest != null)
                    if (!player.Character.CompletedQuests.Contains(requiredQuest.Id))
                        validQuestProgress = false;
            }

            if (!string.IsNullOrEmpty(QuestInProgressRequired))
            {
                var requiredQuest = QuestCatalog.QuestCatalogs.FirstOrDefault(q => q.Value.Name == QuestInProgressRequired).Value;

                if (requiredQuest != null)
                    if (player.Character.QuestLog.FirstOrDefault(q => q.Id == requiredQuest.Id) == null)
                        validQuestProgress = false;
            }

            if (validQuestProgress)
                SendInteractionUpdate();
        }
    }

    public void RemovePhysicalInteractor(Player player, string interactionId)
    {
        if (!CurrentPhysicalInteractors.Contains(interactionId)) return;

        if (PlayerHasPet(player, out var pet))
            if (pet != null)
                pet.CoopTriggerableId = string.Empty;

        CurrentPhysicalInteractors.Remove(interactionId);
        SendInteractionUpdate();
    }

    public bool PlayerHasPet(Player player, out PetModel pet) =>
        player.Character.Pets.TryGetValue(player.GetEquippedPetId(ServerRConfig), out pet) && !pet.InCoopState() &&
                (InteractType == InteractionType.PetChain || InteractType == InteractionType.PetSwitch);


    public void SendInteractionUpdate()
    {
        if (TriggerReceiverActivated() && StayTriggeredOnReceiverActivated) return;

        var tUpdate = new TriggerUpdate_SyncEvent(new SyncEvent(Id.ToString(), SyncEvent.EventType.TriggerUpdate, Room.Time));
        tUpdate.EventDataList.Add(Interactions);
        Room.SendSyncEvent(tUpdate);
    }

    public int GetPhysicalInteractorCount() => CurrentPhysicalInteractors.Count;
    public string[] GetPhysicalInteractorIds() => [.. CurrentPhysicalInteractors];

    public virtual void Triggered(Player player, bool isSuccess, bool isActive)
    {

    }

    public void TriggerInteraction(ActivationType type, Player player)
    {
        if (!Activations.Contains(type))
            return;

        CurrentInteractions++;

        RunTrigger(player);
    }

    public void RunTrigger(Player player)
    {
        var players = Room.GetPlayers();

        // GoTo must be outside for if someone in the room has interacted with the trigger in the past (i.e. in public rooms like CTS)
        if (player != null)
            foreach (var rPlayer in players.Where(x => CurrentPhysicalInteractors.Contains(x.GameObjectId)))
            {
                if (rPlayer == null)
                    continue;

                rPlayer.CheckObjective(ObjectiveEnum.Goto, Id, PrefabName, 1, QuestCatalog);
                rPlayer.CheckObjective(ObjectiveEnum.HiddenGoto, Id, PrefabName, 1, QuestCatalog);
            }

        if (!IsActive)
        {
            if (NbInteractionsMatchesNbPlayers && (Interactions < players.Length || player == null) ||
                    Interactions < NbInteractionsNeeded)
                return;

            Trigger(player, true, true);

            if (DisabledAfterActivation)
                IsEnabled = false;

            LastActivationTime = Room.Time;
        }
        else
        {
            var triggerReceiverActivated = TriggerReceiverActivated();

            if (StayTriggeredOnReceiverActivated && triggerReceiverActivated)
                return;

            if (StayTriggeredOnUnpressed || player.Character.Pets.TryGetValue(player.GetEquippedPetId(ServerRConfig), out var pet)
                && Id == pet.CoopTriggerableId && pet.InCoopState())
                return;

            if (LastActivationTime + ActivationTimeAfterFirstInteraction > Room.Time && ActivationTimeAfterFirstInteraction > 0)
                return;

            Trigger(player, true, false);
        }

        LogTrigger();
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
            sb.AppendLine($"Disabled After Activation: {DisabledAfterActivation}");

        if (NbInteractionsNeeded > 0)
            sb.AppendLine($"Interactions: {Interactions}/{NbInteractionsNeeded}");

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
            sb.AppendLine($"Stay Triggered On Unpressed: {StayTriggeredOnUnpressed}");

        if (StayTriggeredOnReceiverActivated)
            sb.AppendLine($"Stay Triggered Receiver Activated: {StayTriggeredOnReceiverActivated}");

        if (Flip)
            sb.AppendLine($"Flip: {Flip}");

        if (!string.IsNullOrEmpty(ActiveMessage))
            sb.AppendLine($"Active Message: {ActiveMessage}");

        if (SendActiveMessageToObjectId > 0)
            sb.AppendLine($"Send Active Message To Object Id: {SendActiveMessageToObjectId}");

        if (!string.IsNullOrEmpty(DeactiveMessage))
            sb.AppendLine($"Deactive Message: {DeactiveMessage}");

        if (!string.IsNullOrEmpty(TimerSound) && TimerSound != "PF_FX_Timer")
            sb.AppendLine($"Timer Sound: {TimerSound}");

        if (!string.IsNullOrEmpty(TimerEndSound) && TimerEndSound != "PF_FX_Timer_End")
            sb.AppendLine($"Timer End Sound: {TimerEndSound}");

        if (!string.IsNullOrEmpty(QuestCompletedRequired))
            sb.AppendLine($"Quest Completed Required: {QuestCompletedRequired}");

        if (!string.IsNullOrEmpty(QuestInProgressRequired))
            sb.AppendLine($"Quest In Progress Required: {QuestInProgressRequired}");

        if (TriggerRepeatDelay > 0)
            sb.AppendLine($"Repeat Delay: {TriggerRepeatDelay}");

        if (InteractType != InteractionType.None)
            sb.Append($"Interaction Type: {InteractType}");

        if (ActivationTimeAfterFirstInteraction > 0)
            sb.AppendLine($"Activation Time After First Interaction: {ActivationTimeAfterFirstInteraction}");

        FileLogger.WriteGenericLog<TriggerCoopController>("triggered-coop", $"[Trigger {Id}]", sb.ToString(), LoggerType.Trace);
    }

    public void Trigger(Player player, bool success, bool active)
    {
        IsActive = active;

        foreach (var trigger in Triggers)
        {
            var triggers = Room.GetEntitiesFromId<ICoopTriggered>(trigger.Key);

            if (triggers.Length > 0)
                foreach (var triggeredEntity in triggers)
                    triggeredEntity.TriggerStateChange(trigger.Value, IsActive, player.GameObjectId);
            else
                LogTriggerErrors(trigger.Key, trigger.Value);
        }

        if (player != null)
        {
            Room.SentEntityTriggered(Id, player, success, IsActive);
            Triggered(player, success, IsActive);
        }
    }

    public void LogTriggerErrors(string triggerId, TriggerType type)
    {
        var sb2 = new StringBuilder();

        sb2.AppendLine($"State: {type}")
            .AppendLine($"Components: {Room.GetUnknownComponentTypes(triggerId)}");

        FileLogger.WriteGenericLog<TriggerCoopController>("triggered-errors", $"Trigger {Id}", sb2.ToString(),
            LoggerType.Error);
    }

    private bool TriggerReceiverActivated()
    {
        var receivers = Room.GetEntitiesFromType<TriggerReceiverComp>().ToDictionary(x => x.Id, x => x);

        var triggers = Triggers
            .Where(r => r.Value == TriggerType.Activate)
            .Where(trigger => receivers.ContainsKey(trigger.Key));

        return triggers.Any() &&
            triggers.Select(trigger => receivers[trigger.Key])
            .All(receiver => receiver.Activated);
    }

    public void ResetTrigger()
    {
        CurrentPhysicalInteractors.Clear();
        SendInteractionUpdate();
        IsActive = false;
    }

    bool ITriggerComp.IsActive() => IsActive;

    public void QuestAdded(QuestDescription quest, Player player)
    {
        if (QuestInProgressRequired == quest.Name)
            RunTrigger(player);
    }

    public void QuestCompleted(QuestDescription quest, Player player)
    {
        if (QuestCompletedRequired == quest.Name)
            RunTrigger(player);
    }
}
