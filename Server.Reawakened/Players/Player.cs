using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Players;

public class Player : INetStateData
{
    public CharacterModel Character { get; set; }
    public Room Room { get; set; }
    public int GameObjectId { get; set; }

    public UserInfo UserInfo { get; set; }

    public int UserId => UserInfo.UserId;

    public bool OnGround { get; set; }
    public int Direction { get; set; }
    public Vector3Model Position { get; set; }
    public Vector3Model Velocity { get; set; }
    public bool Invincible { get; set; }

    public bool FirstLogin { get; set; }

    public Player(UserInfo userInfo)
    {
        UserInfo = userInfo;

        Position = new Vector3Model();
        Velocity = new Vector3Model();

        Invincible = false;
        FirstLogin = true;
    }

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        if (Room == null)
            return;

        if (!Room.LevelInfo.IsValid())
            return;

        var roomName = Room.LevelInfo.Name;

        if (!string.IsNullOrEmpty(roomName))
            logger.LogDebug("Dumped player with ID '{User}' from room '{Room}'", UserId, roomName);

        Room.DumpPlayerToLobby(GameObjectId);
    }
}
