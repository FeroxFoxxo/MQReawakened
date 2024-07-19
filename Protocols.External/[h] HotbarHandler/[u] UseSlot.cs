using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
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
    public ILogger<PlayerStatus> Logger { get; set; }

    public override void Run(string[] message)
    {
        var hotbarSlotId = int.Parse(message[5]);
        var targetUserId = int.Parse(message[6]);

        var posX = Convert.ToSingle(message[7]);
        var posY = Convert.ToSingle(message[8]);
        var posZ = Convert.ToSingle(message[9]);

        var direction = Player.TempData.Direction;

        var position = new Vector3()
        {
            x = posX,
            y = posY,
            z = posZ
        };

        var slotItem = Player.Character.Hotbar.HotbarButtons[hotbarSlotId];
        var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);

        Logger.LogDebug("Player used hotbar slot {hotbarId} on {userId} at coordinates {position}",
            hotbarSlotId, targetUserId, position);
        switch (usedItem.ItemActionType)
        {
            case ItemActionType.Drop:
                Player.HandleDrop(ItemRConfig, TimerThread, Logger, usedItem, position, direction);
                Player.UseItemFromHotBar(usedItem.ItemId, ItemCatalog, ItemRConfig);
                break;
            case ItemActionType.Grenade:
            case ItemActionType.Throw:
                HandleRangedWeapon(usedItem, position, direction);
                break;
            case ItemActionType.Genericusing:
            case ItemActionType.Drink:
            case ItemActionType.Eat:
                HandleConsumable(usedItem);
                break;
            case ItemActionType.Melee:
                HandleMeleeWeapon(usedItem, position, direction);
                break;
            case ItemActionType.Relic:
                HandleRelic(usedItem);
                break;
            case ItemActionType.PetUse:
                if (!Player.Character.Pets.TryGetValue(Player.GetEquippedPetId(ServerRConfig), out var petUse))
                {
                    Logger.LogInformation("Could not find pet for {characterName}!", Player.CharacterName);
                    return;
                }

                if (usedItem.ItemEffects.Any())
                {
                    var petSnackEnergyValue = usedItem.ItemEffects.First().Value;
                    petUse.GainEnergy(Player, petSnackEnergyValue);
                }
                break;
            case ItemActionType.Pet:
                if (!Player.Character.Pets.TryGetValue(Player.GetEquippedPetId(ServerRConfig), out var pet))
                {
                    Logger.LogInformation("Could not find pet for {characterName}!", Player.CharacterName);
                    return;
                }
                pet.HandlePetState(Player, TimerThread, ItemRConfig, Logger);
                break;
            default:
                Logger.LogError("Could not find how to handle item action type {ItemAction} for user {UserId}",
                    usedItem.ItemActionType, targetUserId);
                break;
        }
    }

    private void HandleRelic(ItemDescription usedItem) //Needs rework.
    {
        StatusEffect_SyncEvent itemEffect = null;

        foreach (var effect in usedItem.ItemEffects)
            itemEffect = new StatusEffect_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                    (int)effect.Type, effect.Value, effect.Duration, true, usedItem.PrefabName, false);

        Player.SendSyncEventToPlayer(itemEffect);
    }

    private void HandleConsumable(ItemDescription usedItem)
    {
        Player.HandleItemEffect(usedItem, TimerThread, ItemRConfig, ServerRConfig, Logger);
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

            Player.UseItemFromHotBar(usedItem.ItemId, ItemCatalog, ItemRConfig);
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

    private void HandleRangedWeapon(ItemDescription usedItem, Vector3 position, int direction)
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
            Player.UseItemFromHotBar(usedItem.ItemId, ItemCatalog, ItemRConfig);
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
            Player.Character.CalculateDamage(prjData.UsedItem, ItemCatalog),
            prjData.UsedItem.Elemental, prjData.IsGrenade);

        Player.Room.AddProjectile(prj);
    }

    private void HandleMeleeWeapon(ItemDescription usedItem, Vector3 position, int direction)
    {
        var prjId = Player.Room.CreateProjectileId().ToString();

        // Add weapon stats later
        var prj = new MeleeEntity(prjId, position, Player, direction, 0.51f, usedItem,
            Player.Character.CalculateDamage(usedItem, ItemCatalog),
            usedItem.Elemental, ServerRConfig, ItemRConfig);

        Player.Room.AddProjectile(prj);
    }
}
