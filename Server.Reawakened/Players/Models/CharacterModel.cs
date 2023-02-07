using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Players.Models;

public class CharacterModel
{
    public CharacterDataModel Data { get; set; }

    public int Level { get; set; }

    public int SpawnPoint { get; set; }
    public int PortalId { get; set; }

    public Dictionary<int, List<int>> CollectedIdols { get; set; }
    public List<EmailHeaderModel> Emails { get; set; }

    public CharacterModel()
    {
        CollectedIdols = new Dictionary<int, List<int>>();
        Emails = new List<EmailHeaderModel>();
    }

    public override string ToString() => throw new InvalidOperationException();
}
