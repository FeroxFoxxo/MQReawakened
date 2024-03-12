using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Enums;

namespace Protocols.External._h__HotbarHandler;
public class UseSlot : ExternalProtocol
{
    public override string ProtocolName => "hu";

    public ItemCatalog ItemCatalog { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<PlayerStatus> Logger { get; set; }
    public InternalAchievement InternalAchievement { get; set; }

    public override void Run(string[] message)
    {
        var hotbarSlotId = int.Parse(message[5]);
        var targetUserId = int.Parse(message[6]);
        var position = new Vector3Model()
        {
            X = Convert.ToSingle(message[7]),
            Y = Convert.ToSingle(message[8]),
            Z = Convert.ToSingle(message[9])
        };
        Logger.LogDebug("Player used hotbar slot {hotbarId} on {userId} at coordinates {position}",
            hotbarSlotId, targetUserId, position);
        var direction = Player.TempData.Direction;
        var slotItem = Player.Character.Data.Hotbar.HotbarButtons[hotbarSlotId];
        var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);
        switch (usedItem.ItemActionType)
        {
            case ItemActionType.Drop:
                Player.HandleDrop(ServerRConfig, TimerThread, Logger, usedItem, position, direction);
                break;
            case ItemActionType.Throw:
                HandleRangedWeapon(usedItem, position, direction);
                break;
            case ItemActionType.Genericusing:
            case ItemActionType.Drink:
            case ItemActionType.Eat:
                HandleConsumable(usedItem, hotbarSlotId);
                break;
            case ItemActionType.Melee:
                HandleMeleeWeapon(usedItem, position, direction);
                break;
            case ItemActionType.Pet:
                HandlePet(usedItem);
                break;
            case ItemActionType.Relic:
                HandleRelic(usedItem);
                break;
            default:
                Logger.LogError("Could not find how to handle item action type {ItemAction} for user {UserId}",
                    usedItem.ItemActionType, targetUserId);
                break;
        }
    }

    private void HandlePet(ItemDescription usedItem)
    {
        Player.SendXt("ZE", Player.UserId, usedItem.ItemId, 1);
        Player.Character.Data.PetItemId = usedItem.ItemId;
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
        Player.HandleItemEffect(usedItem, TimerThread, ServerRConfig, Logger);
        var removeFromHotBar = true;

        if (usedItem.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Pets)
            removeFromHotBar = false;

        if (removeFromHotBar)
        {
            if (usedItem.ItemActionType == ItemActionType.Eat)
            {
                Player.CheckAchievement(AchConditionType.Consumable, string.Empty, InternalAchievement, Logger);
                Player.CheckAchievement(AchConditionType.Consumable, usedItem.PrefabName, InternalAchievement, Logger);
            }
            else if (usedItem.ItemActionType == ItemActionType.Drink)
            {
                Player.CheckAchievement(AchConditionType.Drink, string.Empty, InternalAchievement, Logger);
                Player.CheckAchievement(AchConditionType.Drink, usedItem.PrefabName, InternalAchievement, Logger);
            }

            RemoveFromHotBar(Player.Character, usedItem, hotbarSlotId);
        }
    }

    private void HandleRangedWeapon(ItemDescription usedItem, Vector3Model position, int direction)
    {
        var rand = new Random();
        var prjId = Math.Abs(rand.Next()).ToString();
        while (Player.Room.GameObjectIds.Contains(prjId))
            prjId = Math.Abs(rand.Next()).ToString();

        // Add weapon stats later
        var prj = new ProjectileEntity(Player, prjId, position, direction, 3, usedItem,
            Player.Character.Data.CalculateDamage(usedItem, ItemCatalog),
            usedItem.Elemental, ServerRConfig);
        Player.Room.Projectiles.Add(prjId, prj);
    }

    private void HandleMeleeWeapon(ItemDescription usedItem, Vector3Model position, int direction)
    {
        var rand = new Random();
        var prjId = Math.Abs(rand.Next()).ToString();

        while (Player.Room.GameObjectIds.Contains(prjId))
            prjId = Math.Abs(rand.Next()).ToString();

        // Add weapon stats later
        var prj = new MeleeEntity(Player, prjId, position, direction, 3, usedItem,
            Player.Character.Data.CalculateDamage(usedItem, ItemCatalog),
            usedItem.Elemental, ServerRConfig);

        Player.Room.Projectiles.Add(prjId, prj);

        HandleOldMelee(usedItem, position, direction);
    }

    private void HandleOldMelee(ItemDescription usedItem, Vector3Model position, int direction)
    {
        var planeName = Player.GetPlayersPlaneString();

        var rand = new Random();
        var meleeId = Math.Abs(rand.Next());

        var hitEvent = new Melee_SyncEvent(
            Player.GameObjectId.ToString(),
            Player.Room.Time,
            position.X,
            position.Y,
            position.Z,
            direction,
            1,
            1,
            meleeId,
            usedItem.PrefabName
        );

        Player.Room.SendSyncEvent(hitEvent);
        var hitboxWidth = 3f;
        var hitboxHeight = 4f;

        var isLeft = direction > 0;

        var meleeHitbox = new DefaultCollider(
            meleeId.ToString(),
            new Vector3Model()
            {
                X = isLeft ? Player.TempData.Position.X : Player.TempData.Position.X - hitboxWidth,
                Y = Player.TempData.Position.Y,
                Z = Player.TempData.Position.Z
            },
            hitboxWidth,
            hitboxHeight,
            planeName,
            Player.Room
        );

        var weaponDamage = usedItem.GetDamageAmount(Logger, ServerRConfig);

        foreach (var objects in Player.Room.Planes[planeName].GameObjects.Values)
        {
            foreach (var obj in objects)
            {
                var objectId = obj.ObjectInfo.ObjectId;
                var prefabName = obj.ObjectInfo.PrefabName;

                var objCollider = new DefaultCollider(
                    objectId,
                    obj.ObjectInfo.Position,
                    obj.ObjectInfo.Rectangle.Width,
                    obj.ObjectInfo.Rectangle.Height,
                    planeName,
                    Player.Room
                );

                if (isLeft)
                {
                    if (obj.ObjectInfo.Position.X < position.X)
                        continue;
                }
                else
                {
                    if (obj.ObjectInfo.Position.X > position.X)
                        continue;
                }

                var isColliding = meleeHitbox.CheckCollision(objCollider);

                if (isColliding)
                {
                    if (Player.Room.KilledObjects.Contains(obj.ObjectInfo.ObjectId)) 
                        continue;

                    foreach (var triggerCoopEntity in Player.Room.GetEntitiesFromId<TriggerCoopControllerComp>(obj.ObjectInfo.ObjectId))
                        triggerCoopEntity.TriggerInteraction(ActivationType.NormalDamage, Player);

                    foreach (var enemyEntity in Player.Room.GetEntitiesFromId<EnemyControllerComp>(obj.ObjectInfo.ObjectId))
                        enemyEntity.Damage(weaponDamage, Player);
                }
            }
        }
    }

    private void RemoveFromHotBar(CharacterModel character, ItemDescription item, int hotbarSlotId)
    {
        character.Data.Inventory.Items[item.ItemId].Count--;
        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
        {
            character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);
            SendXt("hu", character.Data.Hotbar);
        }
        Player.SendUpdatedInventory(false);
    }
}
