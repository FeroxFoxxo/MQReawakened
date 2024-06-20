using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class MaxHP : SlashCommand
{
    public override string CommandName => "/MaxHP";

    public override string CommandDescription => "Set your monkey's health to it's maximum value.";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Player;

    public override void Execute(Player player, string[] args) =>
        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId, player.Room.Time,
            player.Character.Write.CurrentLife = player.Character.MaxLife, player.Character.MaxLife, player.GameObjectId));
}
