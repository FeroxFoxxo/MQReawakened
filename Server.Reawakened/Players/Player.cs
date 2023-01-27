using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Characters.Models;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Players.Models;

namespace Server.Reawakened.Players;

public class Player : INetStateData
{
    public Level CurrentLevel;
    public int PlayerId;
    public UserInfo UserInfo;
    public int CurrentCharacter;

    public Player(UserInfo userInfo) => UserInfo = userInfo;

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        if (CurrentLevel != null)
        {
            var levelName = CurrentLevel.LevelData.Name;

            if (!string.IsNullOrEmpty(levelName))
                logger.LogDebug("Dumped player with ID '{User}' from level '{Level}'", PlayerId, levelName);
        }

        CurrentLevel?.DumpPlayerToLobby(PlayerId);
    }

    public void JoinLevel(NetState state, Level level)
    {
        CurrentLevel?.RemoveClient(PlayerId);
        CurrentLevel = level;
        CurrentLevel.AddClient(state);
    }

    public int GetLevelId() => CurrentLevel != null ? CurrentLevel.LevelData.LevelId : -1;

    public CharacterDetailedModel GetCurrentCharacter()
        => UserInfo.Characters[CurrentCharacter];
}
