using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Levels.Models;
using Server.Reawakened.Levels.Models.Planes;
using Server.Reawakened.Players.Models;

namespace Server.Reawakened.Players;

public class Player : INetStateData
{
    public int CurrentCharacter { get; set; }
    public int PlayerId { get; set; }
    public Level CurrentLevel { get; set; }
    public UserInfo UserInfo { get; set; }

    public bool OnGround { get; set; }
    public int Direction { get; set; }
    public Vector3Model Position { get; set; }
    public Vector3Model Velocity { get; set; }

    public Player(UserInfo userInfo)
    {
        UserInfo = userInfo;
        Position = new Vector3Model();
        Velocity = new Vector3Model();
    }

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        if (CurrentLevel == null)
            return;

        if (!CurrentLevel.LevelInfo.IsValid())
            return;

        var levelName = CurrentLevel.LevelInfo.Name;

        if (!string.IsNullOrEmpty(levelName))
            logger.LogDebug("Dumped player with ID '{User}' from level '{Level}'", PlayerId, levelName);

        CurrentLevel.DumpPlayerToLobby(PlayerId);
    }
}
