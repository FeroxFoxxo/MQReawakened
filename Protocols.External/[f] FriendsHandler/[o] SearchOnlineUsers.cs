using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;

namespace Protocols.External._f__FriendsHandler;
public class SearchOnlineUsers : ExternalProtocol
{
    public override string ProtocolName => "fo";
    
    public PlayerContainer PlayerContainer { get; set; }

    public override void Run(string[] message)
    {
        var name = string.Join(" ", message[5]);

        Player.SendXt("fo", GetSearchData(name));
    }

    private string GetSearchData(string name)
    {
        var sb = new SeparatedStringBuilder('|');

        foreach (var player in PlayerContainer.GetAllPlayers())
            if (player.CharacterName.Contains(name) || player.CharacterName.Equals(name))
                sb.Append(GetPlayerData(player));

        return sb.ToString();
    }

    private string GetPlayerData(Player player)
    {
        var sb = new SeparatedStringBuilder('>');

        sb.Append(player.CharacterName);
        sb.Append((int)player.UserInfo.Gender);
        sb.Append(player.Character.LevelId);
        sb.Append("1");
        sb.Append(player.Character.GlobalLevel);
        sb.Append(Player.Character.Blocked.Contains(player.CharacterId) ? "1" : "0");
        sb.Append(Player.Character.Muted.Contains(player.CharacterId) ? "1" : "0");
        sb.Append(Player.Character.Friends.Contains(player.CharacterId) ? "1" : "0");
        sb.Append((int)player.Character.InteractionStatus);

        return sb.ToString();
    }
}
