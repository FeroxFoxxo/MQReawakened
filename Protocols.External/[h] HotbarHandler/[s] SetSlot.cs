using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._h__HotbarHandler;

public class SetSlot : ExternalProtocol
{
    public override string ProtocolName => "hs";

    public PetAbilities PetAbilities { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public ILogger<PlayerStatus> Logger { get; set; }

    public override void Run(string[] message)
    {
        var hotbarSlotId = int.Parse(message[5]);
        var itemId = int.Parse(message[6]);

        if (!Player.Character.TryGetItem(itemId, out var item))
        {
            Logger.LogError("Could not find item with ID {itemId} in inventory.", itemId);
            return;
        }

        Player.SetHotbarSlot(hotbarSlotId, item, ItemRConfig);

        if (ItemCatalog.GetItemFromId(itemId).IsPet() &&
            PetAbilities.PetAbilityData.TryGetValue(itemId, out var petAbility))
            Player.EquipPet(petAbility, WorldStatistics, ServerRConfig, ItemCatalog);

        SendXt("hs", Player.Character.Hotbar);
    }
}
