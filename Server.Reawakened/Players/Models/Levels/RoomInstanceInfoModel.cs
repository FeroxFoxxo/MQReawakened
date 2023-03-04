using Server.Base.Core.Models;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Levels;

public class RoomInstanceInfoModel
{
    public string LevelName { get; set; }
    public string HostIp { get; set; }

    public int HostPort { get; set; }

    public string RoomName { get; set; }

    public int CountGroup { get; set; }
    public int CountFriends { get; set; }
    public int CountClan { get; set; }

    public int TotalPopulation { get; set; }

    public RoomInstanceInfoModel(Player player, InternalRwConfig config)
    {
        var character = player.Character;
        var room = player.Room;

        HostIp = config.ServerAddress;
        HostPort = config.Port;

        LevelName = room.LevelInfo.Name;
        RoomName = room.GetRoomName();

        var players = room.Clients
            .Select(c => c.Value.Get<Player>())
            .ToArray();

        CountGroup = player.Group.GroupMembers
            .Where(g => players.Any(p => p.UserId == g.Key))
            .Count();

        CountFriends = character.Data.GetFriends().PlayerList
            .Where(f => players.Any(p => p.UserId == f.CharacterId))
            .Count();

        CountClan = players
            .Where(p => p.Character.Data.Allegiance == character.Data.Allegiance)
            .Count();

        TotalPopulation = players.Length;
    }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(':');

        sb.Append(LevelName);
        sb.Append(HostIp);
        sb.Append(HostPort);
        sb.Append(RoomName);
        sb.Append(CountGroup);
        sb.Append(CountFriends);
        sb.Append(CountClan);
        sb.Append(TotalPopulation);

        return sb.ToString();
    }
}
