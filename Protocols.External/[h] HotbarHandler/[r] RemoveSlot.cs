using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._h__HotbarHandler;

public class RemoveSlot : ExternalProtocol
{
    public override string ProtocolName => "hr";

    public PetAbilities PetAbilities { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<RemoveSlot> Logger { get; set; }

    public override void Run(string[] message)
    {
        var hotbarSlotId = int.Parse(message[5]);

        if (!Player.Character.Hotbar.HotbarButtons.TryGetValue(hotbarSlotId, out var hotbarItem))
        {
            Logger.LogWarning("{characterName} has not yet unlocked slot #{hotbarSlotNum}.)",
                Player.CharacterName, hotbarSlotId++);
            return;
        }
        
        Player.SetEmptySlot(hotbarSlotId);

        if (ItemCatalog.GetItemFromId(hotbarItem.ItemId) != null &&
            ItemCatalog.GetItemFromId(hotbarItem.ItemId).IsPet() &&
            Player.Character.Pets.TryGetValue(hotbarItem.ItemId.ToString(), out var pet) &&
            pet != null && PetAbilities.PetAbilityData.TryGetValue(int.Parse(pet.ItemId), out var petAbilityParams))
            Player.UnequipPet(pet.ItemId, petAbilityParams, ItemCatalog, WorldStatistics, ItemRConfig);

        SendXt("hr", Player.Character.Hotbar);
    }
}
