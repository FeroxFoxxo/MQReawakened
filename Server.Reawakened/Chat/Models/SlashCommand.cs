using A2m.Server;
using Server.Base.Accounts.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Models;
public abstract class SlashCommand
{
    public abstract string CommandName { get; }
    public abstract string CommandDescription { get; }
    public abstract List<ParameterModel> Arguments { get; }
    public abstract AccessLevel AccessLevel { get; }

    public void Log(string message, Player player) =>
        player.Chat(CannedChatChannel.Tell, "Console", message);

    public void Run(Player player, string[] args)
    {
        if (!(player.Account.AccessLevel >= AccessLevel))
            return;

        Execute(player, args);
    }

    public abstract void Execute(Player player, string[] args);
}
