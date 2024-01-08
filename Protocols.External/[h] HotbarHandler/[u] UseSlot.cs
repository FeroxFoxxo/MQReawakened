using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Entity;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._h__HotbarHandler;

public class UseSlot : ExternalProtocol
{
    public override string ProtocolName => "hu";

    public ILogger<UseSlot> Logger { get; set; }

    public ItemCatalog ItemCatalog { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ObjectiveCatalogInt ObjectiveCatalog { get; set; }

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
            case ItemActionType.Throw:
                HandleRangedWeapon(position, direction, usedItem);
                break;
            case ItemActionType.Drink:
            case ItemActionType.Eat:
                HandleConsumable(usedItem, hotbarSlotId);
                break;
            case ItemActionType.Melee:
                HandleMeleeWeapon(usedItem, position, direction);
                break;
            default:
                Logger.LogError("Could not find how to handle item action type {ItemAction} for user {UserId}",
                    usedItem.ItemActionType, targetUserId);
                break;
        }
    }

    private void HandleConsumable(ItemDescription item, int hotbarSlotId)
    {
        foreach (var effect in item.ItemEffects)
        {
            if (effect.Type is ItemEffectType.Invalid or ItemEffectType.Unknown)
                continue;

            var statusEffect = new StatusEffect_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                effect.TypeId, effect.Value, effect.Duration, true, item.PrefabName, true);

            Player.SendSyncEventToPlayer(statusEffect);
        }

        if (!item.UniqueInInventory)
            RemoveFromHotbar(Player.Character, item, hotbarSlotId);
    }

    private void HandleRangedWeapon(Vector3Model position, int direction, ItemDescription usedItem)
    {
        var rand = new Random();
        var prjId = Math.Abs(rand.Next());

        while (Player.Room.GameObjectIds.Contains(prjId))
            prjId = Math.Abs(rand.Next());

        var prj = new ProjectileEntity(Player, prjId, position.X, position.Y, position.Z, direction, 3, usedItem);
        
        Player.Room.Projectiles.Add(prjId, prj);
    }

    private void HandleMeleeWeapon(ItemDescription usedItem, Vector3Model position, int direction)
    {
        var monsters = new List<GameObjectModel>();

        var planeName = position.Z > 10 ? "Plane1" : "Plane0";

        var hitEvent = new Melee_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time, position.X, position.Y, position.Z, direction, 1, 1, 0, usedItem.PrefabName);
        Player.Room.SendSyncEvent(hitEvent);

        position.Z = 0;

        foreach (var obj in
                 Player.Room.Planes[planeName].GameObjects.Values
                     .Where(obj => Vector3Model.Distance(position, obj.ObjectInfo.Position) <= 3.4f)
                )
        {
            var isLeft = direction > 0;

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

            var objectId = obj.ObjectInfo.ObjectId;
            var prefabName = obj.ObjectInfo.PrefabName;

            Logger.LogInformation("Found close game object {PrefabName} with Id {ObjectId}", prefabName, objectId);

            if (Player.Room.Entities.TryGetValue(objectId, out var entityComponents))
                foreach (var component in entityComponents)
                    if (component is TriggerCoopControllerComp triggerCoopEntity)
                        triggerCoopEntity.TriggerInteraction(ActivationType.NormalDamage, Player);

                    else if (component is BreakableEventControllerComp breakableObjEntity)
                        breakableObjEntity.Destroy(Player);

                    else if (component is InterObjStatusComp enemyEntity)
                        enemyEntity.SendDamageEvent(Player);
        }
    }

    private void RemoveFromHotbar(CharacterModel character, ItemDescription item, int hotbarSlotId)
    {
        character.Data.Inventory.Items[item.ItemId].Count--;

        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
        {
            character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);

            SendXt("hu", character.Data.Hotbar);

            character.Data.Inventory.Items[item.ItemId].Count = -1;
        }

        Player.SendUpdatedInventory(false);
    }
}
