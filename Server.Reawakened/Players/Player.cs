using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Levels;
using Server.Reawakened.Players.Models;

namespace Server.Reawakened.Players;

public class Player : INetStateData
{
    public int CurrentCharacter;
    public Level CurrentLevel;
    public int PlayerId;
    public UserInfo UserInfo;

    public Player(UserInfo userInfo) => UserInfo = userInfo;

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        if (CurrentLevel == null)
            return;

        if (!CurrentLevel.LevelData.IsValid())
            return;

        var levelName = CurrentLevel.LevelData.Name;

        if (!string.IsNullOrEmpty(levelName))
            logger.LogDebug("Dumped player with ID '{User}' from level '{Level}'", PlayerId, levelName);

        CurrentLevel.DumpPlayerToLobby(PlayerId);
    }
}
