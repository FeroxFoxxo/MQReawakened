using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Configs;
using UnityEngine;
using Server.Reawakened.Players.Extensions;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Bundles.Base;
using PetDefines;
using Server.Reawakened.Core.Configs;

namespace Server.Reawakened.Players.Models.Pets;

public class PetModel
{
    public string PetId { get; set; }
    public PetAbilityModel PetAbilityModel { get; set; }
    public int MaxEnergy { get; set; }
    public int CurrentEnergy { get; set; }
    public bool InCoopJumpState { get; set; }
    public bool InCoopSwitchState { get; set; }
    public string CurrentTriggerableId { get; set; }

    public void SpawnPet(Player player, bool spawnPet, string petId, PetAbilityParams petAbilityParams,
        bool refillEnergy, WorldStatistics worldStatistics, ServerRConfig serverRConfig)
    {
        if (refillEnergy)
        {
            MaxEnergy = player.GetMaxPetEnergy(worldStatistics);
            CurrentEnergy = player.GetMaxPetEnergy(worldStatistics);
        }

        PetId = petId;
        PetAbilityModel = new PetAbilityModel(petAbilityParams, 0, false, false);

        CurrentTriggerableId = string.Empty;
        InCoopJumpState = false;
        InCoopSwitchState = false;

        player.Character.Data.PetItemId = int.Parse(PetId);

        player.SendXt("ZE", player.UserId, PetId, Convert.ToInt32(spawnPet));
        player.SendXt("Zm", player.UserId, true);
        NotifyPet(player, serverRConfig);
    }

    public static void NotifyPet(Player player, ServerRConfig serverRConfig)
    {
        if (player.Character.Pets.TryGetValue(player.GetEquippedPetId(serverRConfig), out var pet))
            player.SendXt("Za", player.UserId, pet.PetProfile(int.Parse(player.GameObjectId), int.Parse(pet.PetId),
            (int)PetType.coop, pet.CurrentEnergy, pet.CurrentEnergy, 1, 0));
    }

    //Unsure how NotifyAllPets is supposed to work, needs to be looked into more.
    public static void NotifyAllPets(Player player)
    {
        var sb = new SeparatedStringBuilder('>');

        foreach (var pet in player.Character.Pets.Values)
            sb.Append(pet.PetProfile(int.Parse(player.GameObjectId), int.Parse(pet.PetId),
        (int)PetType.coop, pet.CurrentEnergy, pet.CurrentEnergy, 1, 0));

        player.SendXt("Zp", player.UserId, sb.ToString());
    }

    public void HandlePetState(Player player, TimerThread timerThread, ItemRConfig itemRConfig, ILogger<PlayerStatus> logger)
    {
        var newPetState = ChangePetState(player);
        var syncParams = string.Empty;

        switch (newPetState)
        {
            case PetInformation.StateSyncType.Deactivate:
                RemoveTriggerInteraction(player, timerThread, itemRConfig.PetPressButtonDelay);
                break;
            case PetInformation.StateSyncType.PetStateCoopSwitch:
                AddTriggerInteraction(player, timerThread, itemRConfig.PetHoldChainDelay);
                syncParams = CurrentTriggerableId;
                break;

            case PetInformation.StateSyncType.PetStateCoopJump:
                var onButton = false;

                if (!string.IsNullOrEmpty(CurrentTriggerableId))
                {
                    onButton = true;
                    AddTriggerInteraction(player, timerThread, itemRConfig.PetPressButtonDelay);
                }

                syncParams = GetPetPosition(player.TempData.Position, onButton, itemRConfig);
                break;

            case PetInformation.StateSyncType.Unknown:
                logger.LogWarning("Unknown pet state type {petState}", newPetState);
                break;
        }

        player.Room.SendSyncEvent(new PetState_SyncEvent(player.GameObjectId, player.Room.Time, newPetState, syncParams));
    }

    public void UseEnergy(Player player)
    {
        var energyUsed = (int)Math.Ceiling((double)MaxEnergy / PetAbilityModel.PetAbilityParams.UseCount);

        CurrentEnergy -= energyUsed;

        if (CurrentEnergy < 0)
            CurrentEnergy = 0;

        player.SendXt("Zg", player.UserId, CurrentEnergy);

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.PetEnergyValue, energyUsed, (int)PetAbilityModel.PetAbilityParams.Duration, true, player.GameObjectId, false));
    }

    public void GainEnergy(Player player, ItemDescription petFoodItem)
    {
        var itemEffect = petFoodItem.ItemEffects[(int)ItemFilterCategory.Consumables];
        var energyValue = itemEffect.Value;

        CurrentEnergy += energyValue;

        if (CurrentEnergy > MaxEnergy)
            CurrentEnergy = MaxEnergy;

        player.SendXt("Zg", player.UserId, CurrentEnergy);

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.PetRegainEnergy, energyValue, itemEffect.Duration, true, petFoodItem.PrefabName, false));
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
            InCoopJumpState = true;
            return PetInformation.StateSyncType.PetStateCoopJump;
        }

        InCoopSwitchState = true;
        InCoopJumpState = false;
        return PetInformation.StateSyncType.PetStateCoopSwitch;
    }

    public bool InCoopState() => InCoopJumpState || InCoopSwitchState;

    public string GetPetPosition(Vector3 position, bool OnButton, ItemRConfig itemConfig)
    {
        var syncParams = new SeparatedStringBuilder('|');

        var yOffset = OnButton ? 0 : itemConfig.PetPosYOffset;

        syncParams.Append(position.x);
        syncParams.Append(position.y + yOffset);
        syncParams.Append(position.z);

        return syncParams.ToString();
    }

    public string PetProfile(int id, int itemId, int typeId, int energy, int foodToConsume, long timeToConsume, int boostXp)
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
        TriggerCoopController = player.Room.GetEntityFromId<TriggerCoopControllerComp>(CurrentTriggerableId),
        MultiInteractionTrigger = player.Room.GetEntityFromId<MultiInteractionTriggerCoopControllerComp>(CurrentTriggerableId)
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

        else
        {
            triggerData.MultiInteractionTrigger.CurrentInteractions++;
            triggerData.MultiInteractionTrigger.AddPhysicalInteractor(triggerData.Player, PetId);
            triggerData.MultiInteractionTrigger.RunTrigger(triggerData.Player);
        }
    }

    private void RemoveTriggerInteraction(object interactionData)
    {
        var triggerData = (InteractionData)interactionData;

        if (triggerData.TriggerCoopController != null)
        {
            triggerData.TriggerCoopController.CurrentInteractions--;
            triggerData.TriggerCoopController.RemovePhysicalInteractor(triggerData.Player, PetId);
            triggerData.TriggerCoopController.RunTrigger(triggerData.Player);
        }

        else
        {
            triggerData.MultiInteractionTrigger.CurrentInteractions--;
            triggerData.MultiInteractionTrigger.RemovePhysicalInteractor(triggerData.Player, PetId);
            triggerData.MultiInteractionTrigger.RunTrigger(triggerData.Player);
        }

        CurrentTriggerableId = string.Empty;
    }
}
