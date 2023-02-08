using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Levels.Models.Planes;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._i__InventoryHandler;

internal class InventoryItemUse : ExternalProtocol
{
    public override string ProtocolName => "iu";

    public ILogger<InventoryItemUse> Logger { get; set; }

    public VendorCatalog VendorCatalog { get; set; }
    public ItemCatalog ItemCatalog { get; set; }

    public override void Run(string[] message)
    {
        var itemId = Convert.ToInt32(message[5]);

        var item = ItemCatalog.GetItemFromId(itemId);
        if (item == null)
        {
            Logger.LogError("Could not find item with id {itemId}", itemId);
            return;
        }

        var player = NetState.Get<Player>();
        var character = player.GetCurrentCharacter();

        switch (item.SubCategoryId)
        {
            case A2m.Server.ItemSubCategory.Invalid:
                break;
            case A2m.Server.ItemSubCategory.Nut:
                break;
            case A2m.Server.ItemSubCategory.Wood:
                break;
            case A2m.Server.ItemSubCategory.Herbsandflowers:
                break;
            case A2m.Server.ItemSubCategory.Unknown_4:
                break;
            case A2m.Server.ItemSubCategory.Unknown_5:
                break;
            case A2m.Server.ItemSubCategory.Mineral:
                break;
            case A2m.Server.ItemSubCategory.Liquid:
                break;
            case A2m.Server.ItemSubCategory.Potion:
                break;
            case A2m.Server.ItemSubCategory.Usable:
                break;
            case A2m.Server.ItemSubCategory.SlotHead:
                break;
            case A2m.Server.ItemSubCategory.SlotHair:
                break;
            case A2m.Server.ItemSubCategory.SlotBody:
                break;
            case A2m.Server.ItemSubCategory.SlotLegs:
                break;
            case A2m.Server.ItemSubCategory.Ability:
                break;
            case A2m.Server.ItemSubCategory.BMoney:
                break;
            case A2m.Server.ItemSubCategory.CMoney:
                break;
            case A2m.Server.ItemSubCategory.Dye:
                break;
            case A2m.Server.ItemSubCategory.Orb:
                break;
            case A2m.Server.ItemSubCategory.SlotEars:
                break;
            case A2m.Server.ItemSubCategory.SlotUpperFace:
                break;
            case A2m.Server.ItemSubCategory.SlotLowerFace:
                break;
            case A2m.Server.ItemSubCategory.SlotWrist:
                break;
            case A2m.Server.ItemSubCategory.SlotBackpack:
                break;
            case A2m.Server.ItemSubCategory.SlotTail:
                break;
            case A2m.Server.ItemSubCategory.Invisible:
                break;
            case A2m.Server.ItemSubCategory.Fabrics:
                break;
            case A2m.Server.ItemSubCategory.CoreIngredients:
                break;
            case A2m.Server.ItemSubCategory.Defensive:
                break;
            case A2m.Server.ItemSubCategory.Offensive:
                break;
            case A2m.Server.ItemSubCategory.Interactive:
                break;
            case A2m.Server.ItemSubCategory.Collectible:
                break;
            case A2m.Server.ItemSubCategory.Monster:
                break;
            case A2m.Server.ItemSubCategory.Tailoring:
                break;
            case A2m.Server.ItemSubCategory.Alchemy:
                break;
            case A2m.Server.ItemSubCategory.Elixir:
                break;
            case A2m.Server.ItemSubCategory.Bomb:
                break;
            case A2m.Server.ItemSubCategory.Lootable:
                break;
            case A2m.Server.ItemSubCategory.SuperPack:
                {
                    character.RemoveItem(item.ItemId, 1);

                    foreach(var pair in VendorCatalog.GetSuperPacksItemQuantityMap(itemId))
                    {
                        var packItem = ItemCatalog.GetItemFromId(pair.Key);
                        if (packItem == null) continue;

                        character.AddItem(packItem, pair.Value);
                    }

                    NetState.SendUpdatedInventory();
                }
                break;
            case A2m.Server.ItemSubCategory.TrailPass:
                break;
            case A2m.Server.ItemSubCategory.PassiveAbility:
                break;
            case A2m.Server.ItemSubCategory.Housing:
                break;
            case A2m.Server.ItemSubCategory.BananaBox:
                break;
            case A2m.Server.ItemSubCategory.Package:
                break;
            case A2m.Server.ItemSubCategory.MonthlyCollectible:
                break;
            case A2m.Server.ItemSubCategory.Grenade:
                break;
            case A2m.Server.ItemSubCategory.Gem:
                break;
            case A2m.Server.ItemSubCategory.Pet:
                break;
            case A2m.Server.ItemSubCategory.GuestPass:
                break;
            case A2m.Server.ItemSubCategory.PetEggs:
                break;
            case A2m.Server.ItemSubCategory.Pets:
                break;
            case A2m.Server.ItemSubCategory.PetBattlePoints:
                break;
            case A2m.Server.ItemSubCategory.Furniture_Chair:
                break;
            case A2m.Server.ItemSubCategory.Furniture_Table:
                break;
            case A2m.Server.ItemSubCategory.Furniture_Sofa:
                break;
            case A2m.Server.ItemSubCategory.Furniture_Misc:
                break;
            case A2m.Server.ItemSubCategory.Decor_Lighting:
                break;
            case A2m.Server.ItemSubCategory.Decor_Picture:
                break;
            case A2m.Server.ItemSubCategory.Decor_Trophy:
                break;
            case A2m.Server.ItemSubCategory.Decor_Misc:
                break;
            case A2m.Server.ItemSubCategory.Room_Floor:
                break;
            case A2m.Server.ItemSubCategory.Room_Ceiling:
                break;
            case A2m.Server.ItemSubCategory.Room_BackWall:
                break;
            case A2m.Server.ItemSubCategory.Room_RightWall:
                break;
            case A2m.Server.ItemSubCategory.Room_LeftWall:
                break;
            case A2m.Server.ItemSubCategory.Room_SideWall:
                break;
            case A2m.Server.ItemSubCategory.Play_Bouncer:
                break;
            case A2m.Server.ItemSubCategory.Play_Rope:
                break;
            case A2m.Server.ItemSubCategory.Play_Platform:
                break;
            case A2m.Server.ItemSubCategory.Play_Slide:
                break;
            case A2m.Server.ItemSubCategory.Play_Hoop:
                break;
            case A2m.Server.ItemSubCategory.Play_Other:
                break;
            case A2m.Server.ItemSubCategory.Unknown:
                break;
            default:
                break;
        }
    }
}
