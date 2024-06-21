using A2m.Server;
using Server.Base.Accounts.Enums;
using Server.Base.Database.Accounts;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Models;
public abstract class SlashCommand
{
    public abstract string CommandName { get; }
    public abstract string CommandDescription { get; }
    public abstract List<ParameterModel> Parameters { get; }
    public abstract AccessLevel AccessLevel { get; }

    public static void Log(string message, Player player) =>
        player.Chat(CannedChatChannel.Tell, "Console", message);

    public void Run(Player player, string[] args)
    {
        if (!(player.NetState.Get<AccountModel>().AccessLevel >= AccessLevel))
            return;

        Execute(player, args);
    }

    public abstract void Execute(Player player, string[] args);
}
