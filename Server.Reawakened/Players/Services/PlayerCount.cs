using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Services;
public class PlayerCount(ServerConsole serverConsole,
    DatabaseContainer databaseContainer, EventSink sink, ILogger<PlayerCount> logger) : IService
{
    public void Initialize() => sink.WorldLoad += Load;

    public void Load() => 
        serverConsole.AddCommand(
            "playerCount", 
            "Sends the current online player count in console.", 
            NetworkType.Server, 
            SendPlayerCount);

    private void SendPlayerCount(string[] command)
    {
        if (command.Length == 1)
            logger.LogInformation($"Currently online players: {databaseContainer.GetAllPlayers().Count}");

        if (command.Length == 2)
            foreach (var item in databaseContainer.GetAllPlayers())
                logger.LogInformation($"{item.CharacterName} - {item.Room.LevelInfo.InGameName} / {item.Room.LevelInfo.LevelId}");
    }
}
