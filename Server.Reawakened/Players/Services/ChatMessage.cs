using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Services;

public class ChatMessage(ServerConsole serverConsole,
    DatabaseContainer databaseContainer, EventSink eventSink) : IService
{
    public void Initialize()
    {
        serverConsole.AddCommand(
            "sendChat",
            "Sends a chat message to all users in the server.",
            NetworkType.Server,
            SendChat
        );

        eventSink.WorldBroadcast += @event => SendConsoleMessage(@event.Message);
    }

    private void SendChat(string[] command) => SendConsoleMessage(string.Join(' ', command.Skip(1)));

    public void SendConsoleMessage(string message)
    {
        lock (databaseContainer.Lock)
        {
            foreach (var player in databaseContainer.GetAllPlayers())
            {
                player.Chat(CannedChatChannel.Tell, "Console", message);
            }
        }
    }
}
