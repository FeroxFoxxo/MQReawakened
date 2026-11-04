using A2m.Server;
using Microsoft.Extensions.Logging;
using PetDefines;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Models.Timers;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Players.Models.Pets;

public class PetModel()
{
    public string ItemId { get; set; }
    public string GameObjectId { get; set; }
    public string PrefabName { get; set; }
    public PetAbilityParams AbilityParams { get; set; }
    public float AbilityCooldown { get; set; }
    public int MaxEnergy { get; set; }
    public int CurrentEnergy { get; set; }
    public DateTime LastTimePetWasEquipped { get; set; }
    public bool ShieldingPlayer { get; set; }
    public bool GuardingPlayer { get; set; }
    public bool InCoopJumpState { get; set; }
    public bool InCoopSwitchState { get; set; }
    public string MostRecentCoopTriggerId { get; set; }
    public DateTime LastEnergyRegenTime { get; set; }

    // PET SUMMONING
    public void SpawnPet(Player petOwner, PetAbilityParams petAbilityParams,
        bool spawnPet, bool refillEnergy, ItemCatalog itemCatalog, WorldStatistics worldStatistics,
        ItemRConfig itemRConfig)
    {
        ItemId = petOwner.GetItemIdOfEquippedPet();
        AbilityParams = petAbilityParams;
        PrefabName = itemCatalog.GetItemFromId(int.Parse(ItemId)).PrefabName;

        AbilityCooldown = 0;
        MaxEnergy = GetMaxPetEnergy(petOwner, worldStatistics);
        CurrentEnergy = refillEnergy ? MaxEnergy : CurrentEnergy;
        LastTimePetWasEquipped = DateTime.UtcNow;

        ResetPetStates();

        if (!spawnPet)
        {
            petOwner.Room.SendSyncEvent(new PetState_SyncEvent(GameObjectId, petOwner.Room.Time, PetInformation.StateSyncType.PetStateVanish, petOwner.UserId.ToString()));
            DespawnCleanup(petOwner);
        }
        else
        {
            NotifyPet(petOwner, CurrentEnergy, itemRConfig);
            SetEnergy(petOwner);
            CreateGameObjectId(petOwner);
            MostRecentCoopTriggerId = petOwner.TempData.CurrentCoopTriggerId;
        }

        foreach (var playerInRoom in petOwner.Room.GetPlayers())
            playerInRoom.SendXt("ZE", petOwner.UserId, int.Parse(ItemId), spawnPet ? "1" : "0");

        petOwner.TempData.EnemiesInPetAbilityZone = [];
    }

    public void LogoutAndDespawnPet(Player petOwner)
    {
        petOwner.RemoveAssociatedTriggers();
        petOwner.SendXt("ZE", petOwner.UserId, int.Parse(ItemId), "0");
        LastTimePetWasEquipped = DateTime.UtcNow;
        DespawnCleanup(petOwner);
    }

    private void DespawnCleanup(Player petOwner)
    {
        RemoveTriggerInteraction(GetInteractionData(petOwner, MostRecentCoopTriggerId));
        RemoveGameObjectId(petOwner);
    }

    private void ResetPetStates()
    {
        ShieldingPlayer = false;
        GuardingPlayer = false;
        InCoopJumpState = false;
        InCoopSwitchState = false;
    }

    public void RegenEnergy(Player player)
    {
        if (CurrentEnergy >= MaxEnergy || MaxEnergy <= 0)
        {
            LastEnergyRegenTime = DateTime.UtcNow;
            return;
        }

        var totalRegainDelayMinutes = GameFlow.StatisticData.GetGlobalStat(Globals.PetFullEnergyRegainDelay);
        if (totalRegainDelayMinutes <= 0)
            return;

        var now = DateTime.UtcNow;

        if (LastEnergyRegenTime == DateTime.MinValue)
        {
            LastEnergyRegenTime = now;
            return;
        }

        var intervalMinutes = (double)totalRegainDelayMinutes / MaxEnergy;
        var elapsedMinutes = (now - LastEnergyRegenTime).TotalMinutes;

        var energyGained = (int)(elapsedMinutes / intervalMinutes);

        if (energyGained > 0)
        {
            GainEnergy(player, energyGained, false);

            LastEnergyRegenTime = CurrentEnergy >= MaxEnergy ? now : LastEnergyRegenTime.AddMinutes(energyGained * intervalMinutes);
        }
    }

    private void CreateGameObjectId(Player player) => GameObjectId = player.Room.CreatePetGameObjectId();

    public void RemoveGameObjectId(Player player)
    {
        player.Room.RemovePetGameObjectId(GameObjectId);
        GameObjectId = string.Empty;
    }

    private void NotifyPet(Player petOwner, int currentEnergy, ItemRConfig itemRConfig) =>
        petOwner.SendXt("Za", petOwner.UserId, PetProfile(int.Parse(petOwner.GameObjectId),
                int.Parse(ItemId), (int)PetType.coop, CurrentEnergy,
                currentEnergy, (long)itemRConfig.PetSnackConsumptionDuration,
                (int)Math.Ceiling(petOwner.Character.Reputation / itemRConfig.PetSnackBoostXP)));

    //Unsure how NotifyAllPets/Zp is supposed to work, needs to be looked into more.
    public void NotifyAllPets(Player petOwner)
    {
        var sb = new SeparatedStringBuilder('>');

        foreach (var pet in petOwner.Character.Pets.Values)
            sb.Append(PetProfile(int.Parse(petOwner.GameObjectId), int.Parse(pet.ItemId),
                (int)PetType.coop, pet.CurrentEnergy, 0, 0, 0));

        petOwner.SendXt("Zp", petOwner.UserId, sb.ToString());
    }

    private static string PetProfile(int id, int itemId, int typeId, int energy, int foodToConsume, long timeToConsume, int boostXp)
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(id);
        sb.Append(itemId);
        sb.Append(typeId);
        sb.Append(energy);
        sb.Append(foodToConsume);
        sb.Append(timeToConsume);
        sb.Append(boostXp);

        return sb.ToString();
    }

    // PET STATE
    public void HandlePetState(Player petOwner, TimerThread timerThread, ItemRConfig itemRConfig, ILogger<PlayerStatus> logger)
    {
        var stateSyncType = ChangePetState(petOwner);
        var syncParams = string.Empty;

        switch (stateSyncType)
        {
            case PetInformation.StateSyncType.Deactivate:
                RemoveTriggerInteraction(petOwner, MostRecentCoopTriggerId, timerThread, itemRConfig.PetPressButtonDelay);
                AbilityCooldown = petOwner.Room.Time + AbilityParams.CooldownTime;
                syncParams = petOwner.UserId.ToString();
                break;

            case PetInformation.StateSyncType.PetStateCoopSwitch:
                AddTriggerInteraction(petOwner, petOwner.TempData.CurrentCoopTriggerId, timerThread, itemRConfig.PetHoldChainDelay);
                syncParams = MostRecentCoopTriggerId;
                break;

            case PetInformation.StateSyncType.PetStateCoopJump:
                var onButton = IsPlayerOnCoopTrigger(petOwner);
                if (onButton)
                {
                    AddTriggerInteraction(petOwner, petOwner.TempData.CurrentCoopTriggerId, timerThread, itemRConfig.PetPressButtonDelay);
                }
                syncParams = GetPetPosition(petOwner.TempData.CopyPosition(), onButton, itemRConfig);
                break;

            case PetInformation.StateSyncType.Unknown:
            default:
                logger.LogWarning("Unknown pet state type {petState}", stateSyncType);
                break;
        }

        petOwner.Room.SendSyncEvent(new PetState_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time, stateSyncType, syncParams));
    }

    private bool IsPlayerOnCoopTrigger(Player petOwner)
    {
        var triggerId = petOwner.TempData.CurrentCoopTriggerId;
        return !string.IsNullOrEmpty(triggerId) && triggerId != "0" && (petOwner.Room.GetEntitiesFromId<TriggerCoopControllerComp>(triggerId)
                   .Any(coopTrig => coopTrig.CurrentPhysicalInteractors.Contains(petOwner.GameObjectId)) ||
               petOwner.Room.GetEntitiesFromId<MultiInteractionTriggerCoopControllerComp>(triggerId)
                   .Any(coopTrig => coopTrig.CurrentPhysicalInteractors.Contains(petOwner.GameObjectId)));
    }

    private PetInformation.StateSyncType ChangePetState(Player player)
    {
        if (InCoopState())
        {
            InCoopSwitchState = false;
            InCoopJumpState = false;
            return PetInformation.StateSyncType.Deactivate;
        }

        if (player.TempData.OnGround)
        {
            InCoopSwitchState = false;
            InCoopJumpState = true;
            return PetInformation.StateSyncType.PetStateCoopJump;
        }

        InCoopSwitchState = true;
        InCoopJumpState = false;
        return PetInformation.StateSyncType.PetStateCoopSwitch;
    }

    public static string GetPetPosition(Vector3Model position, bool OnButton, ItemRConfig itemConfig)
    {
        var syncParams = new SeparatedStringBuilder('|');
        var yOffset = OnButton ? 0 : itemConfig.PetPosYOffset;

        syncParams.Append(position.X);
        syncParams.Append(position.Y + yOffset);
        syncParams.Append(position.Z);

        return syncParams.ToString();
    }

    public void GainEnergy(Player player, int energyGained, bool sync = true)
    {
        var actualGain = Math.Min(energyGained, MaxEnergy - CurrentEnergy);
        if (actualGain <= 0) return;

        CurrentEnergy += actualGain;
        if (sync)
            SetEnergy(player);

        player.SendSyncEventToPlayer(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.PetRegainEnergy, actualGain, 3, true, player.GameObjectId, false));
    }

    public void SetEnergy(Player player) => player.SendXt("Zg", player.UserId, CurrentEnergy);

    public void GainMaxPetEnergy(Player player, WorldStatistics worldStatistics)
    {
        var maxEnergy = GetMaxPetEnergy(player, worldStatistics);
        GainEnergy(player, maxEnergy);
    }

    public int GetMaxPetEnergy(Player player, WorldStatistics worldStatistics) =>
        worldStatistics.Statistics[ItemEffectType.PetEnergyValue][WorldStatisticsGroup.Pet][player.Character.GlobalLevel];

    public void UseEnergy(Player player)
    {
        var energyUsed = (int)Math.Ceiling((double)MaxEnergy / AbilityParams.UseCount);

        CurrentEnergy = Math.Max(0, CurrentEnergy - energyUsed);
        SetEnergy(player);

        player.SendSyncEventToPlayer(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.PetEnergyValue, energyUsed, 1, true, player.GameObjectId, false));
    }

    public void EatSnack(Player player, int snackEnergy)
    {
        GainEnergy(player, snackEnergy);
        player.SendXt("Zf", player.UserId, CurrentEnergy, int.Parse(ItemId), player.Room.Time);
    }

    public bool InCoopState() => InCoopJumpState || InCoopSwitchState;
    public bool AbilityIsReady(Player player) => AbilityCooldown <= player.Room.Time;
    public bool PetIsBusy(Player player) => InCoopState() || !AbilityIsReady(player);

    // COOP TRIGGERS
    public class InteractionData : PlayerRoomTimer
    {
        public TriggerCoopControllerComp TriggerCoopController { get; set; }
        public MultiInteractionTriggerCoopControllerComp MultiInteractionTrigger { get; set; }

        public override bool IsValid() => base.IsValid() &&
            (TriggerCoopController == null || TriggerCoopController.IsValid()) &&
            (MultiInteractionTrigger == null || MultiInteractionTrigger.IsValid());
    }

    private void AddTriggerInteraction(Player player, string currentTriggerIdFromPlayer, TimerThread timerThread, float delay)
    {
        if (!string.IsNullOrEmpty(currentTriggerIdFromPlayer) && currentTriggerIdFromPlayer != "0")
            timerThread.RunDelayed(AddTriggerInteraction, GetInteractionData(player, currentTriggerIdFromPlayer), TimeSpan.FromSeconds(delay));

        MostRecentCoopTriggerId = currentTriggerIdFromPlayer;
    }

    private void RemoveTriggerInteraction(Player player, string triggerId, TimerThread timerThread, float delay = 0)
    {
        if (!string.IsNullOrEmpty(triggerId) && triggerId != "0")
            timerThread.RunDelayed(RemoveTriggerInteraction, GetInteractionData(player, triggerId), delay != 0 ? TimeSpan.FromSeconds(delay) : TimeSpan.Zero);

        MostRecentCoopTriggerId = string.Empty;
    }

    public InteractionData GetInteractionData(Player player, string triggerId) => new()
    {
        TriggerCoopController = player.Room.GetEntityFromId<TriggerCoopControllerComp>(triggerId),
        MultiInteractionTrigger = player.Room.GetEntityFromId<MultiInteractionTriggerCoopControllerComp>(triggerId),
        Player = player
    };

    private void AddTriggerInteraction(ITimerData data) => ProcessTriggerInteraction(data, isAdding: true);

    public void RemoveTriggerInteraction(ITimerData data) => ProcessTriggerInteraction(data, isAdding: false);

    private void ProcessTriggerInteraction(ITimerData data, bool isAdding)
    {
        if (data is not InteractionData triggerData) return;

        if (triggerData.TriggerCoopController != null)
        {
            var contains = triggerData.TriggerCoopController.CurrentPhysicalInteractors.Contains(GameObjectId);
            if (isAdding && !contains)
            {
                triggerData.TriggerCoopController.CurrentInteractions++;
                triggerData.TriggerCoopController.AddPhysicalInteractor(triggerData.Player, GameObjectId);
                triggerData.TriggerCoopController.RunTrigger(triggerData.Player);
            }
            else if (!isAdding && contains)
            {
                triggerData.TriggerCoopController.CurrentInteractions--;
                triggerData.TriggerCoopController.RemovePhysicalInteractor(triggerData.Player, GameObjectId);
                triggerData.TriggerCoopController.RunTrigger(triggerData.Player);
            }
        }
        else if (triggerData.MultiInteractionTrigger != null)
        {
            var contains = triggerData.MultiInteractionTrigger.CurrentPhysicalInteractors.Contains(GameObjectId);
            if (isAdding && !contains)
            {
                triggerData.MultiInteractionTrigger.CurrentInteractions++;
                triggerData.MultiInteractionTrigger.AddPhysicalInteractor(triggerData.Player, GameObjectId);
                triggerData.MultiInteractionTrigger.RunTrigger(triggerData.Player);
            }
            else if (!isAdding && contains)
            {
                triggerData.MultiInteractionTrigger.CurrentInteractions--;
                triggerData.MultiInteractionTrigger.RemovePhysicalInteractor(triggerData.Player, GameObjectId);
                triggerData.MultiInteractionTrigger.RunTrigger(triggerData.Player);
            }
        }
    }
}
