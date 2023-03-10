using Server.Reawakened.Players;

namespace Server.Reawakened.Chat.Models;

public class ChatCommand
{
    public delegate bool ChatCommandCallback(Player player, string[] args);

    public string Name { get; set; }
    public string Arguments { get; set; }
    public ChatCommandCallback CommandMethod { get; set; }

    public ChatCommand(string name, string arguments, ChatCommandCallback commandMethod)
    {
        CommandMethod = commandMethod;
        Name = name.ToLower();
        Arguments = arguments;
    }
}
