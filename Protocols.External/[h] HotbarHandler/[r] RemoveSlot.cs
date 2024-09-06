using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._h__HotbarHandler;

public class RemoveSlot : ExternalProtocol
{
    public override string ProtocolName => "hr";

    public PetAbilities PetAbilities { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemRConfig ItemConfig { get; set; }
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

        if (Player.Character.Pets.TryGetValue(hotbarItem.ItemId.ToString(), out var pet) && pet.IsEquipped &&
            PetAbilities.PetAbilityData.TryGetValue(int.Parse(pet.PetId), out var petAbilityParams))
            Player.UnequipPet(petAbilityParams, WorldStatistics, ServerRConfig, ItemCatalog);

        Player.SetEmptySlot(hotbarSlotId, ItemConfig);

        SendXt("hr", Player.Character.Hotbar);
    }
}
