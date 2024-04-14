using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Configs;
using UnityEngine;
using PetDefines;

namespace Server.Reawakened.Players.Models.Pets;

public class PetModel()
{
    public PetAbilityParams PetAbilities { get; set; }
    public int Id { get; set; } = 0;
    public float AbilityCooldown { get; set; } = 0f;
    public bool InCoopJumpState { get; set; } = false;
    public bool InCoopSwitchState { get; set; } = false;
    public string CurrentTargetId { get; set; } = string.Empty;
    public int MaxEnergy { get; set; } = 1;
    public int CurrentEnergy { get; set; } = 100000; //testing purposes.
    public int EvolutionLevel { get; set; } = 1;

    public void UseEnergy(Player player, ItemRConfig rConfig)
    {
        var energyUsed = (int)Math.Ceiling(MaxEnergy * rConfig.PetUseEnergyRatio);

        CurrentEnergy -= energyUsed;

        if (CurrentEnergy <= 0)
        {
            CurrentEnergy = 0;
            player.SendXt("Zo", player.UserId);
            return;
        }

        player.SendXt("Za", player.UserId, PetProfile(player.Character.Data.PetItemId,
            -1, (int)PetType.coop, CurrentEnergy, energyUsed, 1, 100));
        player.SendXt("Zg", player.UserId, CurrentEnergy);

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.PetEnergyValue, energyUsed, 3,
            true, player.CharacterName, false));
    }

    public void GainEnergy(Player player, ItemDescription petFoodItem)
    {
        var itemEffect = petFoodItem.ItemEffects[(int)ItemFilterCategory.Consumables];
        var energyValue = itemEffect.Value;

        CurrentEnergy += energyValue;

        if (CurrentEnergy > MaxEnergy)
            CurrentEnergy = MaxEnergy;

        Console.WriteLine("GainEnergyValue: " + energyValue);
        player.SendXt("Za", player.UserId, PetProfile(player.Character.Data.PetItemId, petFoodItem.ItemId,
            (int)PetType.coop, CurrentEnergy, energyValue, itemEffect.Duration, 500));
        player.SendXt("Zg", player.UserId, CurrentEnergy);

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.PetRegainEnergy, energyValue, itemEffect.Duration, true, player.CharacterName, false));
    }

    public class InteractionData()
    {
        public Player Player = null;
        public string TriggeredBy = string.Empty;
        public BaseTriggerCoopController<TriggerCoopController> TriggerCoopController = new TriggerCoopControllerComp();
        public BaseTriggerCoopController<MultiInteractionTriggerCoopController> MultiInteractionTrigger = new MultiInteractionTriggerCoopControllerComp();
    }

    public InteractionData GetInteractionData(Player player, PetModel pet) =>
        new()
        {
            Player = player,
            TriggeredBy = pet.Id.ToString(),
            TriggerCoopController = player.Room?.GetEntityFromId<TriggerCoopControllerComp>(pet.CurrentTargetId),
            MultiInteractionTrigger = player.Room?.GetEntityFromId<MultiInteractionTriggerCoopControllerComp>(pet.CurrentTargetId),
        };

    public void StartTriggerDelay(Player player, PetModel pet, TimerThread timerThread, float delay) =>
        timerThread.DelayCall(TriggerInteraction, GetInteractionData(player, pet), TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);

    public void RemoveTriggerDelay(Player player, PetModel pet, TimerThread timerThread, float delay) =>
        timerThread.DelayCall(RemoveInteraction, GetInteractionData(player, pet), TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);

    public void TriggerInteraction(object interactionData)
    {
        var triggerData = (InteractionData)interactionData;

        triggerData.TriggerCoopController?.AddPhysicalInteractor(triggerData.Player, triggerData.TriggeredBy);
        triggerData.TriggerCoopController?.RunTrigger(triggerData.Player);

        triggerData.MultiInteractionTrigger?.AddPhysicalInteractor(triggerData.Player, triggerData.TriggeredBy);
        triggerData.MultiInteractionTrigger?.RunTrigger(triggerData.Player);
    }

    public void RemoveInteraction(object interactionData)
    {
        var triggerData = (InteractionData)interactionData;

        triggerData.TriggerCoopController?.RemovePhysicalInteractor(triggerData.Player, triggerData.TriggeredBy);
        triggerData.TriggerCoopController?.RunTrigger(triggerData.Player);

        triggerData.MultiInteractionTrigger?.RemovePhysicalInteractor(triggerData.Player, triggerData.TriggeredBy);
        triggerData.MultiInteractionTrigger?.RunTrigger(triggerData.Player);

        triggerData.Player.Character.Pets[int.Parse(triggerData.TriggeredBy)].CurrentTargetId = string.Empty;
    }

    public PetInformation.StateSyncType ChangePetState(Player player, int petId)
    {
        if (PetInCoopState(player, petId))
        {
            player.Character.Pets[petId].InCoopSwitchState = false;
            player.Character.Pets[petId].InCoopJumpState = false;
            return PetInformation.StateSyncType.Deactivate;
        }

        if (player.TempData.OnGround)
        {
            player.Character.Pets[petId].InCoopJumpState = true;
            return PetInformation.StateSyncType.PetStateCoopJump;
        }

        player.Character.Pets[petId].InCoopSwitchState = true;
        return PetInformation.StateSyncType.PetStateCoopSwitch;
    }

    private bool PetInCoopState(Player player, int petId) => player.Character.Pets[petId].InCoopJumpState || player.Character.Pets[petId].InCoopSwitchState;

    public string GetPetPosition(Vector3 position, ItemRConfig itemConfig, bool OnButton)
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
}
