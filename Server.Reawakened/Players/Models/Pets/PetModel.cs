using A2m.Server;
using PetDefines;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using UnityEngine;

namespace Server.Reawakened.Players.Models.Pets;

public class PetModel()
{
    public string PetId { get; set; }
    public PetAbilityParams AbilityParams { get; set; }
    public float AbilityCooldown { get; set; }
    public int MaxEnergy { get; set; }
    public int CurrentEnergy { get; set; }
    public bool InCoopJumpState { get; set; }
    public bool InCoopSwitchState { get; set; }
    public string CurrentTriggerId { get; set; }
    public bool HasGainedOfflineEnergy { get; set; }
    public DateTime LastTimePetWasEquipped { get; set; }

    public void SpawnPet(Player petOwner, string petId, bool spawnPet, PetAbilityParams abilityParams,
        bool refillEnergy, WorldStatistics worldStatistics, ServerRConfig config, TimerThread energyRegenerationTimer)
    {
        PetId = petId;
        AbilityParams = abilityParams;
        MaxEnergy = petOwner.GetMaxPetEnergy(worldStatistics, config);

        if (!spawnPet)
            LastTimePetWasEquipped = DateTime.Now;

        if (!refillEnergy)
        {
            if (!HasGainedOfflineEnergy)
            {
                CurrentEnergy += GetOfflineRegeneratedEnergy(worldStatistics);
                HasGainedOfflineEnergy = true;
            }

            if (CurrentEnergy < MaxEnergy)
                StartEnergyRegeneration(petOwner, energyRegenerationTimer, worldStatistics);
        }

        else
            CurrentEnergy = MaxEnergy;

        AbilityCooldown = petOwner.Room.Time + AbilityParams.CooldownTime;
        InCoopJumpState = false;
        InCoopSwitchState = false;
        CurrentTriggerId = string.Empty;

        NotifyPet(petOwner);

        petOwner.Character.Write.PetItemId = int.Parse(PetId);

        petOwner.SendXt("ZE", petOwner.UserId, PetId, Convert.ToInt32(spawnPet));
        petOwner.SendXt("Zm", petOwner.UserId, true);
    }

    private int GetOfflineRegeneratedEnergy(WorldStatistics worldStatistics)
    {
        var minutesSinceLogOff = (DateTime.Now - LastTimePetWasEquipped).TotalMinutes;
        var regainRate = MaxEnergy / worldStatistics.GlobalStats[Globals.PetFullEnergyRegainDelay];
        var gainedEnergy = (int)Math.Round(minutesSinceLogOff * regainRate);

        if (CurrentEnergy + gainedEnergy > MaxEnergy)
            gainedEnergy = MaxEnergy - CurrentEnergy;

        return gainedEnergy;
    }

    public void StartEnergyRegeneration(Player player, TimerThread energyRegenerationTimer, WorldStatistics worldStatistics)
    {
        var timeToRegainEnergy = worldStatistics.GlobalStats[Globals.PetFullEnergyRegainDelay];
        var interval = timeToRegainEnergy / MaxEnergy;

        player.TempData.PetEnergyRegenTimer?.Stop();
        player.TempData.PetEnergyRegenTimer = energyRegenerationTimer.DelayCall(RegenerateEnergy, player,
            TimeSpan.FromMinutes((double)interval), TimeSpan.FromMinutes((double)interval), MaxEnergy - CurrentEnergy);
    }

    private void RegenerateEnergy(object player)
    {
        var petOwner = (Player)player;

        if (CurrentEnergy >= MaxEnergy)
        {
            petOwner.TempData.PetEnergyRegenTimer?.Stop();
            return;
        }

        CurrentEnergy++;
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
                syncParams = CurrentTriggerId;
                break;

            case PetInformation.StateSyncType.PetStateCoopJump:
                var onButton = false;

                if (!string.IsNullOrEmpty(CurrentTriggerId))
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

    public class InteractionData()
    {
        public Player Player = null;
        public TriggerCoopControllerComp TriggerCoopController = null;
        public MultiInteractionTriggerCoopControllerComp MultiInteractionTrigger = null;
    };

    public InteractionData GetInteractionData(Player player) => new()
    {
        Player = player,
        TriggerCoopController = player.Room.GetEntityFromId<TriggerCoopControllerComp>(CurrentTriggerId),
        MultiInteractionTrigger = player.Room.GetEntityFromId<MultiInteractionTriggerCoopControllerComp>(CurrentTriggerId)
    };

    public void AddTriggerInteraction(Player player, TimerThread timerThread, float delay) =>
       timerThread.DelayCall(AddTriggerInteraction, GetInteractionData(player), TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);

    public void RemoveTriggerInteraction(Player player, TimerThread timerThread, float delay) =>
        timerThread.DelayCall(RemoveTriggerInteraction, GetInteractionData(player), TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);

    private void AddTriggerInteraction(object interactionData)
    {
        var triggerData = (InteractionData)interactionData;

        if (triggerData.TriggerCoopController != null)
        {
            triggerData.TriggerCoopController.CurrentInteractions++;
            triggerData.TriggerCoopController.AddPhysicalInteractor(triggerData.Player, PetId);
            triggerData.TriggerCoopController.RunTrigger(triggerData.Player);
        }

        else if (triggerData.MultiInteractionTrigger != null)
        {
            triggerData.MultiInteractionTrigger.CurrentInteractions++;
            triggerData.MultiInteractionTrigger.AddPhysicalInteractor(triggerData.Player, PetId);
            triggerData.MultiInteractionTrigger.RunTrigger(triggerData.Player);
        }

        else return;
    }

    private void RemoveTriggerInteraction(object interactionData)
    {
        var triggerData = (InteractionData)interactionData;

        if (triggerData.TriggerCoopController != null)
        {
            triggerData.TriggerCoopController.RemovePhysicalInteractor(triggerData.Player, PetId);
            triggerData.TriggerCoopController.RunTrigger(triggerData.Player);
            triggerData.TriggerCoopController.CurrentInteractions--;
        }

        else if (triggerData.MultiInteractionTrigger != null)
        {
            triggerData.MultiInteractionTrigger.RemovePhysicalInteractor(triggerData.Player, PetId);
            triggerData.MultiInteractionTrigger.RunTrigger(triggerData.Player);
            triggerData.MultiInteractionTrigger.CurrentInteractions--;
        }

        else return;
    }
}
