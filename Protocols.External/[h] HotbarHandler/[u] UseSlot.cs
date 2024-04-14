using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Achievements;
using UnityEngine;

namespace Protocols.External._h__HotbarHandler;
public class UseSlot : ExternalProtocol
{
    public override string ProtocolName => "hu";

    public ItemCatalog ItemCatalog { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ILogger<PlayerStatus> Logger { get; set; }

    public override void Run(string[] message)
    {
        var hotbarSlotId = int.Parse(message[5]);
        var targetUserId = int.Parse(message[6]);
        var posX = Convert.ToSingle(message[7]);
        var posY = Convert.ToSingle(message[8]);
        var posZ = Convert.ToSingle(message[9]);

        var direction = Player.TempData.Direction;

        var slotItem = Player.Character.Data.Hotbar.HotbarButtons[hotbarSlotId];
        var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);
        var position = new Vector3()
        {
            x = posX,
            y = posY,
            z = posZ
        };

        Logger.LogDebug("Player used hotbar slot {hotbarId} on {userId} at coordinates {position}",
            hotbarSlotId, targetUserId, position);

        switch (usedItem.ItemActionType)
        {
            case ItemActionType.Drop:
                Player.HandleDrop(ItemRConfig, TimerThread, Logger, usedItem, position, direction);
                RemoveFromHotBar(Player.Character, usedItem, hotbarSlotId);
                break;
            case ItemActionType.Grenade:
            case ItemActionType.Throw:
                HandleRangedWeapon(usedItem, position, direction, hotbarSlotId);
                break;
            case ItemActionType.Genericusing:
            case ItemActionType.Drink:
            case ItemActionType.Eat:
                HandleConsumable(usedItem, hotbarSlotId);
                break;
            case ItemActionType.Melee:
                HandleMeleeWeapon(usedItem, position, direction);
                break;
            case ItemActionType.Relic:
                HandleRelic(usedItem);
                break;
            case ItemActionType.PetUse:
                //Console.WriteLine("1: " + usedItem.ItemEffects[(int)ItemFilterCategory.Consumables].Value + " | 2: " + usedItem.ItemEffects[1].Value);
                Player.Character.Pets[usedItem.ItemId].GainEnergy(Player, usedItem);
                break;
            case ItemActionType.Pet:
                HandlePetState(Player.Character.Pets[usedItem.ItemId]);
                break;
            default:
                Logger.LogError("Could not find how to handle item action type {ItemAction} for user {UserId}",
                    usedItem.ItemActionType, targetUserId);
                break;
        }
    }

    private void HandlePetState(PetModel pet)
    {
        var newPetState = pet.ChangePetState(Player, pet.Id);
        var syncParams = string.Empty;

        Console.WriteLine("State: " + newPetState);
        switch (newPetState)
        {
            case PetInformation.StateSyncType.Deactivate:
                pet.RemoveTriggerDelay(Player, pet, TimerThread, ItemRConfig.PetPressButtonDelay);
                break;
            case PetInformation.StateSyncType.PetStateCoopSwitch:
                pet.StartTriggerDelay(Player, pet, TimerThread, ItemRConfig.PetHoldChainDelay);
                syncParams = pet.CurrentTargetId;
                break;

            case PetInformation.StateSyncType.PetStateCoopJump:
                var onButton = false;

                if (!string.IsNullOrEmpty(pet.CurrentTargetId))
                {
                    onButton = true;
                    pet.StartTriggerDelay(Player, pet, TimerThread, ItemRConfig.PetPressButtonDelay);
                }

                syncParams = pet.GetPetPosition(Player.TempData.Position, ItemRConfig, onButton);
                break;

            case PetInformation.StateSyncType.Unknown:
                Logger.LogWarning("Unknown pet state type {petState}", newPetState);
                break;
        }

        Player.Room.SendSyncEvent(new PetState_SyncEvent(Player.GameObjectId, Player.Room.Time, newPetState, syncParams));
    }

    private void HandleRelic(ItemDescription usedItem) //Needs rework.
    {
        StatusEffect_SyncEvent itemEffect = null;

        foreach (var effect in usedItem.ItemEffects)
            itemEffect = new StatusEffect_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                    (int)effect.Type, effect.Value, effect.Duration, true, usedItem.PrefabName, false);

        Player.SendSyncEventToPlayer(itemEffect);
    }

    private void HandleConsumable(ItemDescription usedItem, int hotbarSlotId)
    {
        Player.HandleItemEffect(usedItem, TimerThread, ItemRConfig, Logger);
        var removeFromHotBar = true;

        if (usedItem.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Pets)
            removeFromHotBar = false;

        if (removeFromHotBar)
        {
            switch (usedItem.ItemActionType)
            {
                case ItemActionType.Eat:
                    Player.CheckAchievement(AchConditionType.Consumable, [usedItem.PrefabName], InternalAchievement, Logger);
                    break;
                case ItemActionType.Drink:
                    Player.CheckAchievement(AchConditionType.Drink, [usedItem.PrefabName], InternalAchievement, Logger);
                    break;
            }

            RemoveFromHotBar(Player.Character, usedItem, hotbarSlotId);
        }
    }

    public class ProjectileData()
    {
        public string ProjectileId;
        public ItemDescription UsedItem;
        public Vector3 Position;
        public int Direction;
        public bool IsGrenade;
    }

    private void HandleRangedWeapon(ItemDescription usedItem, Vector3 position, int direction, int hotbarSlotId)
    {
        var isGrenade = usedItem.SubCategoryId is ItemSubCategory.Grenade or ItemSubCategory.Bomb;

        var projectileData = new ProjectileData()
        {
            ProjectileId = Player.Room.CreateProjectileId().ToString(),
            UsedItem = usedItem,
            Position = position,
            Direction = direction,
            IsGrenade = isGrenade,
        };

        if (isGrenade)
        {
            TimerThread.DelayCall(LaunchProjectile, projectileData, TimeSpan.FromSeconds(ItemRConfig.GrenadeSpawnDelay), TimeSpan.Zero, 1);
            RemoveFromHotBar(Player.Character, usedItem, hotbarSlotId);
        }

        else
            LaunchProjectile(projectileData);
    }

    private void LaunchProjectile(object projectileData)
    {
        var prjData = (ProjectileData)projectileData;

        // Add weapon stats later
        var prj = new GenericProjectile(prjData.ProjectileId, Player, ItemRConfig.GrenadeLifeTime,
            prjData.Position, ItemRConfig, ServerRConfig, prjData.Direction, prjData.UsedItem,
            Player.Character.Data.CalculateDamage(prjData.UsedItem, ItemCatalog),
            prjData.UsedItem.Elemental, prjData.IsGrenade);

        Player.Room.AddProjectile(prj);
    }

    private void HandleMeleeWeapon(ItemDescription usedItem, Vector3 position, int direction)
    {
        var prjId = Player.Room.CreateProjectileId().ToString();

        // Add weapon stats later
        var prj = new MeleeEntity(prjId, position, Player, direction, 0.51f, usedItem,
            Player.Character.Data.CalculateDamage(usedItem, ItemCatalog),
            usedItem.Elemental, ServerRConfig, ItemRConfig);

        Player.Room.AddProjectile(prj);
    }

    private void RemoveFromHotBar(CharacterModel character, ItemDescription item, int hotbarSlotId)
    {
        var itemModel = character.Data.Inventory.Items[item.ItemId];
        itemModel.Count--;

        if (itemModel.Count <= 0)
        {
            Player.RemoveHotbarSlot(hotbarSlotId, ItemCatalog, WorldStatistics);
            SendXt("hu", character.Data.Hotbar);
        }

        Player.SendUpdatedInventory();
    }
}
