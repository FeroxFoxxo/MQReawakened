using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Item;
public class ResetEquipment : SlashCommand
{
    public override string CommandName => "/resetequipment";

    public override string CommandDescription => "Reset all of your character's equipment.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public CharacterHandler CharacterHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        foreach (var characterId in player.UserInfo.CharacterIds)
        {
            var target = CharacterHandler.GetCharacterFromId(characterId);

            if (player.Character != null && target == player.Character)
                return;

            foreach (var equippedItem in target.Equipment.EquippedItems)
                target.Inventory.Items.TryAdd(equippedItem.Value, new ItemModel()
                {
                    ItemId = equippedItem.Value,
                    Count = 1,
                    BindingCount = 0,
                    DelayUseExpiry = DateTime.Now
                });

            foreach (var equippedItem in target.Hotbar.HotbarButtons)
                target.Inventory.Items.TryAdd(equippedItem.Value.ItemId, equippedItem.Value);

            if (target.PetItemId > 0)
                target.Write.PetItemId = 0;

            target.Hotbar.HotbarButtons.Clear();
            target.Equipment.EquippedItems.Clear();
            target.Equipment.EquippedBinding.Clear();
        }

        Log("Reset all character's armor.", player);
    }
}
