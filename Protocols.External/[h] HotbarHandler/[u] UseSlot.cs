using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._h__HotbarHandler;

public class UseSlot : ExternalProtocol
{
    public override string ProtocolName => "hu";

    public ILogger<UseSlot> Logger { get; set; }

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

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

        character.SendUpdatedInventory(NetState, false);

        if (character.Data.Inventory.Items[item.ItemId].Count < 0)
            character.Data.Inventory.Items.Remove(item.ItemId);
    }

    private void HandleMeleeWeapon(Vector3Model position)
    {
        var player = NetState.Get<Player>();

        var planes = new[] { "Plane1", "Plane0" };

        foreach (var planeName in planes)
        {
            foreach (var obj in
                     player.CurrentRoom.Planes[planeName].GameObjects.Values
                         .Where(obj => Vector3Model.Distance(position, obj.ObjectInfo.Position) <= 3f)
                    )
            {
                switch (obj.ObjectInfo.PrefabName)
                {
                    case "PF_GLB_SwitchWall02":
                        var triggerEvent = new Trigger_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                            player.CurrentRoom.Time, true, player.GameObjectId.ToString(), true);

                        player.CurrentRoom.SendSyncEvent(triggerEvent);

                        foreach (var syncedEntity in player.CurrentRoom.Entities[obj.ObjectInfo.ObjectId]
                                     .Where(syncedEntity =>
                                         typeof(AbstractTriggerCoop<>).IsAssignableTo(syncedEntity.GetType())
                                     )
                                )
                        {
                            syncedEntity.RunSyncedEvent(triggerEvent, NetState);
                            break;
                        }

                        return;
                    case "PF_CRS_BARREL01":
                        var aiEvent = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(),
                            player.CurrentRoom.Time,
                            0, 0, 0, 0, "now", false, false);

                        player.CurrentRoom.SendSyncEvent(aiEvent);

                        return;
                    default:
                        Logger.LogDebug("Hit Object: {name}, ObjectId: {id}",
                            obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);
                        break;
                }
            }
        }
    }
}
