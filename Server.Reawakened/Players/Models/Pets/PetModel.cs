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
using Server.Reawakened.Rooms.Models.Timers;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Players.Models.Pets;

public class PetModel()
{
    public string PetId { get; set; }
    public PetAbilityParams AbilityParams { get; set; }
    public bool IsEquipped { get; set; }
    public float AbilityCooldown { get; set; }
    public int MaxEnergy { get; set; }
    public int CurrentEnergy { get; set; }
    public bool InCoopJumpState { get; set; }
    public bool InCoopSwitchState { get; set; }
    public string CoopTriggerableId { get; set; }
    public bool HasGainedOfflineEnergy { get; set; }
    public DateTime LastTimePetWasEquipped { get; set; }

    public void SpawnPet(Player petOwner, PetAbilityParams petAbilityParams,
        bool refillEnergy, WorldStatistics worldStatistics, ServerRConfig serverRConfig)
    {
        ResetPetData(petOwner, petAbilityParams, refillEnergy, worldStatistics, serverRConfig);
        NotifyPet(petOwner);
        petOwner.SendXt("ZE", petOwner.UserId, PetId, Convert.ToInt32(true));
    }

    public void DespawnPet(Player petOwner, PetAbilityParams petAbilityParams,
        WorldStatistics worldStatistics, ServerRConfig config)
    {
        ResetPetData(petOwner, petAbilityParams, false, worldStatistics, config);
        petOwner.SendXt("ZE", petOwner.UserId, PetId, Convert.ToInt32(false));
    }

    private void ResetPetData(Player petOwner, PetAbilityParams petAbilityParams,
        bool refillEnergy, WorldStatistics worldStatistics, ServerRConfig config)
    {
        PetId = petOwner.GetEquippedPetId(config);
        AbilityParams = petAbilityParams;

        AbilityCooldown = 0;
        MaxEnergy = petOwner.GetMaxPetEnergy(worldStatistics, config);

        if (refillEnergy)
            CurrentEnergy = MaxEnergy;

        InCoopJumpState = false;
        InCoopSwitchState = false;
        CoopTriggerableId = string.Empty;
    }

    public void StartEnergyRegeneration(Player player, TimerThread energyRegenerationTimer, WorldStatistics worldStatistics)
    {
        var timeToRegainEnergy = worldStatistics.GlobalStats[Globals.PetFullEnergyRegainDelay];
        var interval = timeToRegainEnergy / MaxEnergy;

        player.TempData.PetEnergyRegenTimer?.Stop();

        player.TempData.PetEnergyRegenTimer = energyRegenerationTimer.RunInterval(
            RegenerateEnergy, new PetTimer() { Pet = this, Player = player },
            TimeSpan.FromMinutes((double)interval), MaxEnergy - CurrentEnergy, TimeSpan.FromMinutes((double)interval)
        );
    }

    public class PetTimer : PlayerTimer
    {
        public PetModel Pet { get; set; }
    }

    private static void RegenerateEnergy(ITimerData data)
    {
        if (data is not PetTimer timer)
            return;

        if (timer.Pet.CurrentEnergy >= timer.Pet.MaxEnergy)
        {
            timer.Player.TempData.PetEnergyRegenTimer?.Stop();
            return;
        }

        timer.Pet.CurrentEnergy++;
    }

    //Might be used for pet snacks instead.
    public void NotifyPet(Player petOwner) =>
        petOwner.SendXt("Za", petOwner.UserId, PetProfile(int.Parse(petOwner.GameObjectId),
                int.Parse(PetId), (int)PetType.coop, CurrentEnergy, 0, 0, 0));

    //Unsure how NotifyAllPets/Zp is supposed to work, needs to be looked into more.
    public static void NotifyAllPets(Player petOwner)
    {
        var sb = new SeparatedStringBuilder('>');

        foreach (var pet in petOwner.Character.Pets.Values)
            sb.Append(PetProfile(int.Parse(petOwner.GameObjectId), int.Parse(pet.PetId),
        (int)PetType.coop, pet.CurrentEnergy, 0, 0, 0));

        petOwner.SendXt("Zp", petOwner.UserId, sb.ToString());
    }

    public void HandlePetState(Player petOwner, TimerThread timerThread, ItemRConfig itemRConfig, ILogger<PlayerStatus> logger)
    {
        var newPetState = ChangePetState(petOwner);
        var syncParams = string.Empty;

        switch (newPetState)
        {
            case PetInformation.StateSyncType.Deactivate:
                RemoveTriggerInteraction(petOwner, timerThread, itemRConfig.PetPressButtonDelay);
                AbilityCooldown = petOwner.Room.Time + AbilityParams.CooldownTime;
                break;
            case PetInformation.StateSyncType.PetStateCoopSwitch:
                AddTriggerInteraction(petOwner, timerThread, itemRConfig.PetHoldChainDelay);
                syncParams = CoopTriggerableId;
                break;

            case PetInformation.StateSyncType.PetStateCoopJump:
                var onButton = false;

                if (!string.IsNullOrEmpty(CoopTriggerableId))
                {
                    onButton = true;
                    AddTriggerInteraction(petOwner, timerThread, itemRConfig.PetPressButtonDelay);
                }

                syncParams = GetPetPosition(petOwner.TempData.Position, onButton, itemRConfig);
                break;
            case PetInformation.StateSyncType.Unknown:
            default:
                logger.LogWarning("Unknown pet state type {petState}", newPetState);
                break;
        }

        petOwner.Room.SendSyncEvent(new PetState_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time, newPetState, syncParams));
    }

    public void UseEnergy(Player player)
    {
        var energyUsed = (int)Math.Ceiling((double)MaxEnergy / AbilityParams.UseCount);

        CurrentEnergy -= energyUsed;

        if (CurrentEnergy < 0)
            CurrentEnergy = 0;

        player.SendXt("Zg", player.UserId, CurrentEnergy);

        player.SendSyncEventToPlayer(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.PetEnergyValue, energyUsed, 1, true, player.GameObjectId, false));
    }

    public void GainEnergy(Player player, int energyAmount)
    {
        CurrentEnergy += energyAmount;

        if (CurrentEnergy > MaxEnergy)
            CurrentEnergy = MaxEnergy;

        player.SendXt("Zg", player.UserId, CurrentEnergy);

        player.SendSyncEventToPlayer(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.PetRegainEnergy, energyAmount, 1, true, player.GameObjectId, false));
    }

    public PetInformation.StateSyncType ChangePetState(Player player)
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

    public bool InCoopState() => InCoopJumpState || InCoopSwitchState;

    public static string GetPetPosition(Vector3 position, bool OnButton, ItemRConfig itemConfig)
    {
        var syncParams = new SeparatedStringBuilder('|');

        var yOffset = OnButton ? 0 : itemConfig.PetPosYOffset;

        syncParams.Append(position.x);
        syncParams.Append(position.y + yOffset);
        syncParams.Append(position.z);

        return syncParams.ToString();
    }

    public static string PetProfile(int id, int itemId, int typeId, int energy, int foodToConsume, long timeToConsume, int boostXp)
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

    public class InteractionData : PlayerRoomTimer
    {
        public TriggerCoopControllerComp TriggerCoopController { get; set; }
        public MultiInteractionTriggerCoopControllerComp MultiInteractionTrigger { get; set; }
        public string PetId { get; set; }

        public override bool IsValid() => base.IsValid() &&
            (TriggerCoopController == null || TriggerCoopController.IsValid()) &&
            (MultiInteractionTrigger == null || MultiInteractionTrigger.IsValid());
    };

    public InteractionData GetInteractionData(Player player) => new()
    {
        TriggerCoopController = player.Room.GetEntityFromId<TriggerCoopControllerComp>(CoopTriggerableId),
        MultiInteractionTrigger = player.Room.GetEntityFromId<MultiInteractionTriggerCoopControllerComp>(CoopTriggerableId),
        Player = player,
        PetId = PetId
    };

    public void AddTriggerInteraction(Player player, TimerThread timerThread, float delay) =>
       timerThread.RunDelayed(AddTriggerInteraction, GetInteractionData(player), TimeSpan.FromSeconds(delay));

    public void RemoveTriggerInteraction(Player player, TimerThread timerThread, float delay) =>
        timerThread.RunDelayed(RemoveTriggerInteraction, GetInteractionData(player), TimeSpan.FromSeconds(delay));

    private static void AddTriggerInteraction(ITimerData data)
    {
        if (data is not InteractionData trigger)
            return;

        if (trigger.TriggerCoopController != null)
        {
            trigger.TriggerCoopController.CurrentInteractions++;
            trigger.TriggerCoopController.AddPhysicalInteractor(trigger.Player, trigger.PetId);
            trigger.TriggerCoopController.RunTrigger(trigger.Player);
        }
        else if (trigger.MultiInteractionTrigger != null)
        {
            trigger.MultiInteractionTrigger.CurrentInteractions++;
            trigger.MultiInteractionTrigger.AddPhysicalInteractor(trigger.Player, trigger.PetId);
            trigger.MultiInteractionTrigger.RunTrigger(trigger.Player);
        }

        else return;
    }

    private static void RemoveTriggerInteraction(ITimerData data)
    {
        if (data is not InteractionData trigger)
            return;

        if (trigger.TriggerCoopController != null)
        {
            trigger.TriggerCoopController.RemovePhysicalInteractor(trigger.Player, trigger.PetId);
            trigger.TriggerCoopController.RunTrigger(trigger.Player);
            trigger.TriggerCoopController.CurrentInteractions--;
        }

        else if (trigger.MultiInteractionTrigger != null)
        {
            trigger.MultiInteractionTrigger.RemovePhysicalInteractor(trigger.Player, trigger.PetId);
            trigger.MultiInteractionTrigger.RunTrigger(trigger.Player);
            trigger.MultiInteractionTrigger.CurrentInteractions--;
        }

        else return;
    }
}
