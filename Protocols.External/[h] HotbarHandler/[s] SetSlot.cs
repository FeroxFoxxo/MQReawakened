using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._h__HotbarHandler;

public class SetSlot : ExternalProtocol
{
    public override string ProtocolName => "hs";

    public ItemCatalog ItemCatalog { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public PetAbilities PetAbilities { get; set; }
    public ILogger<SetSlot> Logger { get; set; }

    public override void Run(string[] message)
    {
        var character = Player.Character;

        var hotbarSlotId = int.Parse(message[5]);
        var itemId = int.Parse(message[6]);

        if (!character.TryGetItem(itemId, out var item))
        {
            Logger.LogError("Could not find item with ID {itemId} in inventory.", itemId);
            return;
        }

        Player.SetHotbarSlot(hotbarSlotId, item, ItemCatalog, WorldStatistics);

        var itemDescription = ItemCatalog.GetItemFromId(itemId);

        if (itemDescription.IsPet() && Player.Character.Pets.ContainsKey(itemId))
            Player.Character.Pets[itemId].PetAbilities = PetAbilities.PetAbilityData[itemId];

        SendXt("hs", character.Data.Hotbar);
    }
}
