using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Web.Apps.Leaderboards.Services;

public class LeaderboardHandler(EventSink sink, InternalLeaderboards leaderboards) : IService
{
    public LeaderBoardGameJson Games { get; private set; }

    public Dictionary<int, CharacterModel> CharacterCache;

    public void Initialize() => sink.WorldLoad += LoadLeaderboard;

    private void LoadLeaderboard()
    {
        Games = new LeaderBoardGameJson
        {
            status = true,
            games = [.. leaderboards.Games]
        };
        
        CharacterCache = [];
    }
}
