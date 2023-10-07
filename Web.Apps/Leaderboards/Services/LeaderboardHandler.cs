using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;

namespace Web.Apps.Leaderboards.Services;

public class LeaderboardHandler(EventSink sink) : IService
{
    public LeaderBoardGameJson Games { get; private set; }

    public void Initialize() => sink.WorldLoad += LoadLeaderboard;

    private void LoadLeaderboard() =>
        Games = new LeaderBoardGameJson
        {
            status = true,
            games = new List<LeaderBoardGameJson.Game>()
        };
}
