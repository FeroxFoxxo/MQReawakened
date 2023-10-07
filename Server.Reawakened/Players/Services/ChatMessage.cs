using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Services;

public class ChatMessage(ServerConsole serverConsole,
    PlayerHandler playerHandler, EventSink eventSink) : IService
{
    private readonly PlayerHandler _playerHandler = playerHandler;
    private readonly ServerConsole _serverConsole = serverConsole;
    private readonly EventSink _eventSink = eventSink;

    public void Initialize()
    {
        _serverConsole.AddCommand(
            "sendChat",
            "Sends a chat message to all users in the server.",
            NetworkType.Server,
            SendChat
        );

        _eventSink.WorldBroadcast += @event => SendConsoleMessage(@event.Message);
    }

    private void SendChat(string[] command) => SendConsoleMessage(string.Join(' ', command.Skip(1)));

    public void SendConsoleMessage(string message)
    {
        lock (_playerHandler.Lock)
        {
            foreach (var player in _playerHandler.PlayerList)
            {
                player.Chat(CannedChatChannel.Tell, "Console", message);
            }
        }
    }
}
