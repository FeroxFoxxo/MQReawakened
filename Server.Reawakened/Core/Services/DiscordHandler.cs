using A2m.Server;
using Discord;
using Discord.WebSocket;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Core.Services;
public class DiscordHandler(ServerRwConfig rwConfig, PlayerContainer playerContainer) : IService
{
    private DiscordSocketClient socketClient;

    private string botToken;
    private ulong channelId;

    public void Initialize()
    {
        botToken = rwConfig.DiscordBotToken;
        channelId = rwConfig.ChannelId;

        if (string.IsNullOrEmpty(botToken))
            return;

        socketClient = new DiscordSocketClient();
        socketClient.MessageReceived += ClientOnMessageReceived;

        socketClient.LoginAsync(TokenType.Bot, botToken);
        socketClient.StartAsync();
    }

    public void SendMessage(string author, string message)
    {
        if (socketClient == null || string.IsNullOrEmpty(botToken))
            return;

        var socketChannel = (ISocketMessageChannel)socketClient.GetChannel(channelId);

        socketChannel.SendMessageAsync(author + ": " + message);
    }

    private async Task ClientOnMessageReceived(SocketMessage socketMessage) =>
        await Task.Run(() =>
        {
            if (!socketMessage.Author.IsBot)
            {
                var author = socketMessage.Author.Username;
                var channelId = socketMessage.Channel.Id;
                var socketChannel = (ISocketMessageChannel)socketClient.GetChannel(channelId);

                var messageId = socketMessage.Id;
                var message = socketChannel.GetMessageAsync(messageId);

                if (this.channelId != socketChannel.Id)
                    return;

                Log(message.Result.Content, author);
            }
        });

    private void Log(string message, string author)
    {
        lock (PlayerContainer.Lock)
        {
            foreach (
                var client in
                from client in playerContainer.GetAllPlayers()
                select client
            )
                client.Chat(CannedChatChannel.Speak, "Discord > " + author, message);
        }
    }
}
