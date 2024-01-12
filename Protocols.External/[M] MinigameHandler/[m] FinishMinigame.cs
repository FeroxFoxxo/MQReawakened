using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.BundlesInternal;
using static LeaderBoardTopScoresJson;

namespace Protocols.External._M__MinigameHandler;

public class FinishedMinigame : ExternalProtocol
{
    public override string ProtocolName => "Mm";

    public ILogger<FinishedMinigame> Logger { get; set; }

    public override void Run(string[] message)
    {
        var minigameId = int.Parse(message[5]);
        var finishedRaceTime = float.Parse(message[6]);

        Logger.LogInformation("Minigame with ID ({minigameId}) has completed.", minigameId);

        var playersInRace = ArenaModel.Participants;

        foreach (var player in playersInRace)
            player.SendXt("Mt", minigameId, Player.CharacterId, finishedRaceTime);

        if (Player.TempData.ArenaModel.BestTimeForLevel == null)
        {
            Player.TempData.ArenaModel.BestTimeForLevel = [];
            Player.TempData.ArenaModel.BestTimeForLevel.Add(Player.Room.LevelInfo.Name, finishedRaceTime);
        }

        if (Player.TempData.ArenaModel.BestTimeForLevel.TryGetValue(Player.Room.LevelInfo.Name, out var time))
            if (finishedRaceTime < time)
            {
                Player.TempData.ArenaModel.BestTimeForLevel[Player.Room.LevelInfo.Name] = finishedRaceTime;
                Player.SendXt("Ms", Player.Room.LevelInfo.InGameName);
            }

        Player.TempData.ArenaModel.ActivatedSwitch = false;

        if (playersInRace.All(p => !p.TempData.ArenaModel.ActivatedSwitch))
            foreach (var player in playersInRace)
                ArenaModel.FinishMinigame(minigameId, player);
    }   
}
