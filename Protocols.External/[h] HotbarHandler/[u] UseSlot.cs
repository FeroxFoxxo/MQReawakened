using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.LootHandlers;
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
            case ItemActionType.Eat:
                Player.Character.RemoveItem(usedItem, 1);
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
        character.Data.Inventory.Items[item.ItemId].Count--;

        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
        {
            character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);

            SendXt("hu", character.Data.Hotbar);

            character.Data.Inventory.Items[item.ItemId].Count = -1;
        }

        Player.SendUpdatedInventory(false);

        if (character.Data.Inventory.Items[item.ItemId].Count < 0)
            character.Data.Inventory.Items.Remove(item.ItemId);
    }

    private void HandleMeleeWeapon(Vector3Model position)
    {
        AiHealth_SyncEvent aiEvent = null;
        Trigger_SyncEvent triggerEvent = null;

        var planes = new[] { "Plane1", "Plane0" };

        foreach (var planeName in planes)
        {
            foreach (var obj in
                     Player.Room.Planes[planeName].GameObjects.Values
                         .Where(obj => Vector3Model.Distance(position, obj.ObjectInfo.Position) <= 3f)
                    )
            {

                switch (obj.ObjectInfo.PrefabName)
                {
                    case "PF_GLB_SwitchWall02":
                        triggerEvent = new Trigger_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                            Player.Room.Time, true, Player.CharacterName, true);

                        Player.Room.SendSyncEvent(triggerEvent);

                        //Temporary way to open closed gates associated to the PF_GLB_SwitchWall02 game object.
                        var genericGate = Player.Room.Planes[planeName].GameObjects.Values
                            .FirstOrDefault(obj => obj.ObjectInfo.PrefabName == "PF_GLB_DoorGeneric01");

                        var triggerGate = new TriggerReceiver_SyncEvent(genericGate.ObjectInfo.ObjectId.ToString(),
                            Player.Room.Time, Player.CharacterName, true, 1);

                        Player.Room.SendSyncEvent(triggerGate);

                        foreach (var syncedEntity in Player.Room.Entities[obj.ObjectInfo.ObjectId]
                                     .Where(syncedEntity =>
                                         typeof(AbstractTriggerCoop<>).IsAssignableTo(syncedEntity.GetType())
                                     )
                                )
                        {
                            syncedEntity.RunSyncedEvent(triggerEvent, Player);
                            break;
                        }

                        return;
                    case "PF_CRS_BARREL01":
                        aiEvent = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                            Player.Room.Time, 0, 100, 0, 0, "now", false, false);

                        Logger.LogInformation("Object name: {args1} Object Id: {args2}",
                            obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);

                        Player.Room.SendSyncEvent(aiEvent);

                        Player.GrantLoot(obj.ObjectInfo.ObjectId,
                            InternalLootCatalog, ItemCatalog, Logger);

                        return;
                    case "PF_Spite_Crawler_Rock":
                        aiEvent = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                            Player.Room.Time, 0, 100, 0, 0, "now", false, true);

                        Logger.LogInformation("Object name: {args1} Object Id: {args2}",
                            obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);

                        Player.Room.SendSyncEvent(aiEvent);
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
    }
}
