using A2m.Server;
using Microsoft.Extensions.Logging;
using Protocols.External._i__InventoryHandler;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.BundlesInternal;
using UnityEngine;

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
        var character = Player.Character;
        var player = Player;

        var hotbarSlotId = int.Parse(message[5]);
        var targetUserId = int.Parse(message[6]);

        var position = new Vector3Model()
        {
            X = Convert.ToSingle(message[7]),
            Y = Convert.ToSingle(message[8]),
            Z = Convert.ToSingle(message[9])
        };

        Console.WriteLine("X: " + position.X);
        Console.WriteLine("Y: " + position.Y);
        Console.WriteLine("Z: " + position.Z);

        var direction = Player.TempData.Direction;

        var slotItem = character.Data.Hotbar.HotbarButtons[hotbarSlotId];
        var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);

        switch (usedItem.ItemActionType)
        {
            case ItemActionType.Throw:
                HandleRangedWeapon(position, player, direction, usedItem);
                break;
            case ItemActionType.Drink:
            case ItemActionType.Eat:
                HandleConsumable(character, usedItem, hotbarSlotId);
                break;
            case ItemActionType.Melee:
                HandleMeleeWeapon(position, direction);
                break;
            default:
                Logger.LogError("Could not find how to handle item action type {ItemAction} for user {UserId}",
                    usedItem.ItemActionType, targetUserId);
                break;
        }
    }

    private void HandleConsumable(CharacterModel character, ItemDescription item, int hotbarSlotId)
    {
        foreach (var effect in item.ItemEffects)
        {
            if (effect.Type is ItemEffectType.Invalid or ItemEffectType.Unknown)
                continue;

            var statusEffect = new StatusEffect_SyncEvent(Player.GameObjectId.ToString(), Player.Room.Time,
                effect.TypeId, effect.Value, effect.Duration, true, Player.GameObjectId.ToString(), true);

            Player.SendSyncEventToPlayer(statusEffect);
        }

        if (!item.UniqueInInventory)
            RemoveFromHotbar(character, item, hotbarSlotId);
    }

    private void HandleRangedWeapon(Vector3Model position, Player player, int playerDirection, ItemDescription usedItem)
    {
        var bulletDirection = 0;

        if (playerDirection == 1)
        {
            position.X -= 3;
            bulletDirection = 7;
        }

        else if (playerDirection == -1)
        {
            position.X += 3;
            bulletDirection = -7;
        }

        var prefabName = usedItem.PrefabName;

        var projectile = new LaunchItem_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            position.X, position.Y + 1, position.Z, bulletDirection, 0, 0, 0, prefabName);

        player.Room.SendSyncEvent(projectile);
    }

    private void HandleMeleeWeapon(Vector3Model position, int direction)
    {
        AiHealth_SyncEvent aiEvent = null;

        var monsters = new List<GameObjectModel>();

        var planeName = position.Z > 10 ? "Plane1" : "Plane0";
        position.Z = 0;

        foreach (var obj in
                 Player.Room.Planes[planeName].GameObjects.Values
                     .Where(obj => Vector3Model.Distance(position, obj.ObjectInfo.Position) <= 3.4f)
                )
        {
            if (direction > 0)
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

            switch (prefabName)
            {
                case "PF_R01_Barrel01":
                case "PF_SHD_Barrel01":
                case "PF_CRS_BarrelNewbZone01":
                case "PF_CRS_BARREL01":
                case "PF_OUT_BARREL03":
                case "PF_WLD_Breakable01":
                case "PF_BON_BreakableUrn01":
                case "PF_EVT_HallwnPumpkinFloatBRK":
                case "PF_EVT_XmasBrkIceCubeLoot01":
                case "PF_EVT_XmasBrkIceCubeLoot02":
                case "PF_EVT_XmasBrkIceCube02":
                    DestroyObject(obj);
                    break;
            }

            aiEvent = new AiHealth_SyncEvent(objectId.ToString(),
                Player.Room.Time, 0, 100, 0, 0, "now", false, true);

            foreach (var entinty in Player.Room.Entities.Values)
                foreach (var component in entinty)
                    if (component is HazardControllerComp triggerCoopEntity ||
                        component.PrefabName.Contains("Spawner")) //Temp until BreakableEventControllerComp is added.              
                        Player.Room.SendSyncEvent(aiEvent);

        }
    }

    private void DestroyObject(GameObjectModel obj)
    {
        var random = new System.Random();
        var randomItem = random.Next(1, 4);
        var itemReward = 0;
        switch (randomItem)
        {
            case 1:
                itemReward = 404;
                break;
            case 2:
                itemReward = 1568;
                break;
            case 3:
                itemReward = 510;
                break;
        }

        var aiEvent = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                        Player.Room.Time, 0, 100, 0, 0, "now", false, false);

        Logger.LogInformation("Object name: {args1} Object Id: {args2}",
            obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);

        Player.Room.SendSyncEvent(aiEvent);

        if (obj.ObjectInfo.PrefabName != "PF_EVT_XmasBrkIceCube02" && obj.ObjectInfo.PrefabName != "PF_EVT_HallwnPumpkinFloatBRK")
            Player.Character.AddItem(ItemCatalog.GetItemFromId(itemReward), 1);

        switch (randomItem)
        {
            case 1:
                itemReward = 1613;
                break;
            case 2:
                itemReward = 1618;
                break;
            case 3:
                itemReward = 1616;
                break;
        }

        if (obj.ObjectInfo.PrefabName == "PF_EVT_HallwnPumpkinFloatBRK")
            Player.Character.AddItem(ItemCatalog.GetItemFromId(itemReward), 1);

        Player.SendUpdatedInventory(false);
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
