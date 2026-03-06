using LitJson;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Web.Apps.Leaderboards.Services;

public class LeaderboardHandler(EventSink sink, InternalLeaderboards leaderboards) : IService
{
    public LeaderBoardGameJson Games { get; private set; }

    public void Initialize() => sink.WorldLoad += LoadLeaderboard;

    private void LoadLeaderboard() =>
        Games = new LeaderBoardGameJson
        {
            status = true,
            games = [.. leaderboards.Games]
        };
}
