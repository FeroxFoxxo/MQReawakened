using A2m.Server;
using Server.Base.Database.Accounts;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Models;
public abstract class SlashCommand : CommandModel
{
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
