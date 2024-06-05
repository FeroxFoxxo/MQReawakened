using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Item;
public class ResetArmor : SlashCommand
{
    public override string CommandName => "/ResetArmor";

    public override string CommandDescription => "Reset all of your character's armor.";

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
                target.Data.Inventory.Items.Add(equippedItem.Value, new ItemModel()
                {
                    ItemId = equippedItem.Value,
                    Count = 1,
                    BindingCount = 0,
                    DelayUseExpiry = DateTime.Now
                });

            target.Data.Equipment.EquippedItems.Clear();
            target.Data.Equipment.EquippedBinding.Clear();
        }

        Log("Reset all character's armor.", player);
    }
}
