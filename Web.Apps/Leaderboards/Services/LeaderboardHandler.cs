using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Web.Apps.Leaderboards.Services;

public class LeaderboardHandler(EventSink sink, InternalLeaderboards leaderboards, ServerRConfig serverRConfig) : IService
{
    public LeaderBoardGameJson Games { get; private set; }

    public void Initialize() => sink.WorldLoad += LoadLeaderboard;

    private void LoadLeaderboard() =>
        Games = new LeaderBoardGameJson
        {
            status = true,
            games = serverRConfig.GameVersion >= GameVersion.vPetMasters2014 ? [..leaderboards.Games] : []
        };
}
