using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Item;
public class ResetEquipment : SlashCommand
{
    public override string CommandName => "/ResetEquipment";

    public override string CommandDescription => "Reset all of your character's equipment.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public CharacterHandler CharacterHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        foreach (var characterId in player.UserInfo.CharacterIds)
        {
            var target = CharacterHandler.Get(characterId);

            if (player.Character != null && target == player.Character)
                return;

            foreach (var equippedItem in target.Data.Equipment.EquippedItems)
                target.Data.Inventory.Items.TryAdd(equippedItem.Value, new ItemModel()
                {
                    ItemId = equippedItem.Value,
                    Count = 1,
                    BindingCount = 0,
                    DelayUseExpiry = DateTime.Now
                });

            foreach (var equippedItem in target.Data.Hotbar.HotbarButtons)
                target.Data.Inventory.Items.TryAdd(equippedItem.Value.ItemId, equippedItem.Value);

            if (target.Data.PetItemId > 0)
                target.Data.PetItemId = 0;

            target.Data.Hotbar.HotbarButtons.Clear();
            target.Data.Equipment.EquippedItems.Clear();
            target.Data.Equipment.EquippedBinding.Clear();
        }

        Log("Reset all character's armor.", player);
    }
}
