using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Services;
public class Coords(ServerConsole serverConsole,
    PlayerContainer playerContainer, EventSink sink, ILogger<Coords> logger) : IService
{
    public void Initialize() => sink.WorldLoad += Load;

    public void Load() => serverConsole.AddCommand(
            "coords",
            "Sends the current online _player positions in console.",
            NetworkType.Server,
            GetPlayerCoords);

    private void GetPlayerCoords(string[] command)
    {
        foreach (var player in playerContainer.GetAllPlayers())
            logger.LogInformation("{PlayerName} - {LevelName} - {X}, {Y}, {Z}", player.CharacterName, player.Room.LevelInfo.InGameName, player.TempData.Position.X, player.TempData.Position.Y, player.TempData.Position.Z);
    }
}
