using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._h__HotbarHandler;

public class UseSlot : ExternalProtocol
{
    public override string ProtocolName => "hu";

    public ILogger<UseSlot> Logger { get; set; }

    public ItemCatalog ItemCatalog { get; set; }

    public InternalLootCatalog InternalLootCatalog { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var hotbarSlotId = int.Parse(message[5]);
        var targetUserId = int.Parse(message[6]);

        var position = new Vector3Model
        {
            X = Convert.ToSingle(message[7]),
            Y = Convert.ToSingle(message[8]),
            Z = Convert.ToSingle(message[9])
        };

        var slotItem = character.Data.Hotbar.HotbarButtons[hotbarSlotId];
        var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);

        switch (usedItem.ItemActionType)
        {
            case ItemActionType.Drink:
                HandleConsumable(character, usedItem, hotbarSlotId);
                break;
            case ItemActionType.Eat:
                HandleConsumable(character, usedItem, hotbarSlotId);
                break;
            case ItemActionType.Melee:
                HandleMeleeWeapon(position);
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
                effect.TypeId, effect.Value, effect.Duration,
                true, Player.GameObjectId.ToString(), true);

            Player.SendSyncEventToPlayer(statusEffect);
        }

        if (!item.UniqueInInventory)
            RemoveFromHotbar(character, item, hotbarSlotId);
    }

    private void HandleMeleeWeapon(Vector3Model position)
    {
        AiHealth_SyncEvent aiEvent = null;

        Console.WriteLine(position);

        var planeName = position.Z > 10 ? "Plane1" : "Plane0";
        position.Z = 0;

        foreach (var obj in
                 Player.Room.Planes[planeName].GameObjects.Values
                     .Where(obj => Vector3Model.Distance(position, obj.ObjectInfo.Position) <= 3f)
                )
        {
            Logger.LogInformation("Found close game object {PrefabName}", obj.ObjectInfo.PrefabName);

            if (Player.Room.Entities.TryGetValue(obj.ObjectInfo.ObjectId, out var entityComponents))
                foreach (var component in entityComponents)
                    if (component is TriggerCoopControllerComp triggerCoopEntity)
                        triggerCoopEntity.TriggerInteraction(ActivationType.NormalDamage, Player);

            switch (obj.ObjectInfo.PrefabName)
            {
                case "PF_CRS_BarrelNewbZone01":
                case "PF_CRS_BARREL01":
                    aiEvent = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                        Player.Room.Time, 0, 100, 0, 0, "now", false, false);

                    Logger.LogInformation("Object name: {args1} Object Id: {args2}",
                        obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);

                    Player.Room.SendSyncEvent(aiEvent);

                    Player.Character.AddItem(ItemCatalog.GetItemFromId(1568), 1);
                    Player.SendUpdatedInventory(false);
                    break;
                case "PF_Spite_Crawler_Rock":
                    aiEvent = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                        Player.Room.Time, 0, 100, 0, 0, "now", false, true);

                    Logger.LogInformation("Object name: {args1} Object Id: {args2}",
                        obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);

                    Player.Room.SendSyncEvent(aiEvent);

                    Player.Character.AddItem(ItemCatalog.GetItemFromId(404), 1);
                    Player.SendUpdatedInventory(false);
                    break;
                case "PF_Spite_Bathog_Rock":
                    aiEvent = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                        Player.Room.Time, 0, 100, 0, 0, "now", false, true);

                    Logger.LogInformation("Object name: {args1} Object Id: {args2}",
                        obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);

                    Player.Room.SendSyncEvent(aiEvent);
                    break;
                case "PF_UniversalSpawnerNewb01":
                    aiEvent = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                        Player.Room.Time, 0, 100, 0, 0, "now", false, true);

                    Logger.LogInformation("Object name: {args1} Object Id: {args2}",
                        obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);

                    Player.Room.SendSyncEvent(aiEvent);
                    break;
                default:
                    Logger.LogInformation("Hit Object: {name}, ObjectId: {id}",
                        obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);
                    break;
            }
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
