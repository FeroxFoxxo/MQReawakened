using Server.Reawakened.Players;

namespace Server.Reawakened.Chat.Models;

public class ChatCommand(string name, string arguments, ChatCommand.ChatCommandCallback commandMethod)
{
    public delegate bool ChatCommandCallback(Player player, string[] args);

    public string Name { get; set; } = name.ToLower();
    public string Arguments { get; set; } = arguments;
    public ChatCommandCallback CommandMethod { get; set; } = commandMethod;
}
