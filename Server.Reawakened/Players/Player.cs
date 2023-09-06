using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Groups;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Players;

public class Player : INetStateData
{
    public PlayerHandler PlayerHandler { get; }
    public NetState NetState { get; }
    public UserInfo UserInfo { get; }

    public int UserId => UserInfo.UserId;

    public GroupModel Group { get; set; }

    public CharacterModel Character { get; set; }
    public bool FirstLogin { get; set; }

    public Room Room { get; set; }
    public int GameObjectId { get; set; }

    public bool OnGround { get; set; }
    public int Direction { get; set; }
    public Vector3Model Position { get; set; }
    public Vector3Model Velocity { get; set; }
    public bool Invincible { get; set; }

    public Player(UserInfo userInfo, NetState state, PlayerHandler playerHandler)
    {
        PlayerHandler = playerHandler;
        NetState = state;
        UserInfo = userInfo;

        Position = new Vector3Model();
        Velocity = new Vector3Model();

        Invincible = false;
        FirstLogin = true;
    }

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger) => Remove(logger);

    public void Remove(Microsoft.Extensions.Logging.ILogger logger)
    {
        this.RemoveFromGroup();

        PlayerHandler.RemovePlayer(this);

        if (Character != null)
            foreach (var player in PlayerHandler.PlayerList.Where(p => Character.Data.FriendList.ContainsKey(p.UserId)))
                player.SendXt("fz", Character.Data.CharacterName);

        if (Room == null)
            return;

        if (!Room.LevelInfo.IsValid())
            return;

        Room.DumpPlayerToLobby(this);

        var roomName = Room.LevelInfo.Name;

        if (!string.IsNullOrEmpty(roomName))
            logger.LogDebug("Dumped player with ID '{User}' from room '{Room}'", UserId, roomName);

        Character = null;
    }
}
