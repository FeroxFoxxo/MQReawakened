using Server.Base.Core.Configs;
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
        RoomName = room.ToString();

        var players = room.GetPlayers();

        CountGroup = player.TempData.Group != null
            ? player.TempData.Group.GetMembers()
                .Where(g => players.Any(p => p.UserId == g.UserId))
                .Count()
            : 0;

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
