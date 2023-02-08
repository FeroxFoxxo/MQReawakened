using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Entities;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

using A2m.Server;
using Server.Reawakened.Levels.Models.Planes;
using static LeaderBoardTopScoresJson;
using Server.Reawakened.Players.Models;
using System.Globalization;

namespace Protocols.External._h__HotbarHandler;

internal class HotbarSlotUse : ExternalProtocol
{
    public override string ProtocolName => "hu";

    public ILogger<HotbarSlotUse> Logger { get; set; }

    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var player = NetState.Get<Player>();

        var character = player.GetCurrentCharacter();

        // position where to spawn FX
        var position = new Vector3Model()
        {
            X = Convert.ToSingle(message[7], CultureInfo.InvariantCulture),
            Y = Convert.ToSingle(message[8], CultureInfo.InvariantCulture),
            Z = Convert.ToSingle(message[9], CultureInfo.InvariantCulture)
        };

        if (int.TryParse(message[5], out var hotbarSlotId))
        {
            //var targetUserId = Convert.ToInt32(message[6]); // Target user
            var slotItem = character.Data.Hotbar.HotbarButtons[hotbarSlotId];
            
            var usedItem = ItemCatalog.GetItemFromId(slotItem.ItemId);

            switch(usedItem.ItemActionType)
            {
                case ItemActionType.Drink:
                case ItemActionType.Eat:
                    HandleConsumable(character, usedItem, hotbarSlotId);
                    break;
            
                case ItemActionType.Melee:
                    HandleMeleeWeapon(usedItem, position);
                    break;

                default:
                    File.AppendAllText("./use.txt", $"{usedItem.SubCategoryId}\n");
                    break;
            }
        }
        else Logger.LogError("HotbarSlot ID must be an integer.");
    }

    private void HandleConsumable(CharacterModel character, ItemDescription item, int hotbarSlotId)
    {
        character.Data.Inventory.Items[item.ItemId].Count--;

        if (character.Data.Inventory.Items[item.ItemId].Count <= 0)
        {
            character.Data.Hotbar.HotbarButtons.Remove(hotbarSlotId);
            SendXt("hu", character.Data.Hotbar);

            character.Data.Inventory.Items[item.ItemId].Count = -1;
            SendXt("ip", character.Data.Inventory.ToString().Replace('>', '|'), false);

            character.Data.Inventory.Items.Remove(item.ItemId);
        }
        else SendXt("ip", character.Data.Inventory.ToString().Replace('>', '|'), false);
    }

    private void HandleMeleeWeapon(ItemDescription item, Vector3Model position)
    {
        var player = NetState.Get<Player>();
        
        //var meleeTrigger = new Trigger_SyncEvent("13160481", player.CurrentLevel.Time, true, player.PlayerId.ToString(), true);
        
        var planes = new string[] { "Plane1", "Plane0" };

        foreach(var planeName in planes)
        {
            foreach(var obj in player.CurrentLevel.LevelPlanes.Planes[planeName].GameObjects.Values)
            {
                if (Vector3Model.Distance(position, obj.ObjectInfo.Position) <= 3f)
                {
                    switch(obj.ObjectInfo.PrefabName)
                    {
                        case "PF_GLB_SwitchWall02":
                            {
                                var ev = new Trigger_SyncEvent(obj.ObjectInfo.ObjectId.ToString(), player.CurrentLevel.Time, true, player.PlayerId.ToString(), true);
                                NetState.SendSyncEventToPlayer(ev);
                                player.CurrentLevel.SendSyncEvent(ev);

                                foreach(var syncEntt in player.CurrentLevel.LevelEntities.Entities[obj.ObjectInfo.ObjectId])
                                {
                                    if (syncEntt is TriggerCoopControllerEntity triggetEntt)
                                    {
                                        triggetEntt.RunSyncedEvent(ev, NetState);
                                        break;
                                    }
                                }

                                return;
                            }
                        case "PF_CRS_BARREL01":
                            {
                                var ev = new AiHealth_SyncEvent(obj.ObjectInfo.ObjectId.ToString(), player.CurrentLevel.Time, 0, 0, 0, 0, "now", false, false);
                                NetState.SendSyncEventToPlayer(ev);
                                player.CurrentLevel.SendSyncEvent(ev);
                                return;
                            }
                        default:
                            Logger.LogDebug("Hit Object: {name}, ObjectId: {id}", obj.ObjectInfo.PrefabName, obj.ObjectInfo.ObjectId);
                            break;
                    }
                }
            }
        }
    }
}
