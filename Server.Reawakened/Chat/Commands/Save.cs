using Server.Base.Accounts.Enums;
using Server.Base.Worlds.Services;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands;
public class Save : SlashCommand
{
    public override string CommandName => "/Save";

    public override string CommandDescription => "Saves all player data. (Owner only)";

    public override List<ParameterModel> Parameters => [];

    public override AccessLevel AccessLevel => AccessLevel.Owner;
    
    public AutoSave AutoSave { get; set; }

    public override void Execute(Player player, string[] args) => AutoSave.Save();
}
