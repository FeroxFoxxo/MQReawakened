using System.Text;

namespace Server.Reawakened.Characters.Models;

public class FriendDataModel
{
    public const char FieldSeparator = '>';

    public string CharacterName { get; set; }
    public int CharacterId { get; set; }
    public bool IsOnline { get; set; }
    public int Level { get; set; }
    public string Location { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsMuted { get; set; }
    public int InteractionStatus { get; set; }

    public FriendDataModel() {}

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(CharacterId);
        sb.Append(FieldSeparator);
        sb.Append(CharacterName);
        sb.Append(FieldSeparator);
        sb.Append(IsOnline ? "1" : "0");
        sb.Append(FieldSeparator); 
        sb.Append(Level);
        sb.Append(FieldSeparator);
        sb.Append(Location);
        sb.Append(FieldSeparator);
        sb.Append(IsBlocked ? "1" : "0");
        sb.Append(FieldSeparator); 
        sb.Append(IsMuted ? "1" : "0");
        sb.Append(FieldSeparator); 
        sb.Append(InteractionStatus);

        return sb.ToString();
    }
}
