using A2m.Server;
using Server.Reawakened.Core.Enums;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Character;

public class CharacterLightModel(CharacterDbEntry entry, GameVersion version)
{
    public CharacterDbEntry Write => entry;

    public CharacterCustomDataModel Customization => new(entry);
    public EquipmentModel Equipment => new(entry);

    public int Id => entry.Id;

    public int PetItemId => entry.PetItemId;
    public bool Registered => entry.Registered;
    public string CharacterName => entry.CharacterName;
    public int UserUuid => entry.UserUuid;
    public int Gender => entry.Gender;
    public int MaxLife => entry.MaxLife;
    public int CurrentLife => entry.CurrentLife;
    public int GlobalLevel => entry.GlobalLevel;
    public CharacterLightData.InteractionStatus InteractionStatus => entry.InteractionStatus;
    public TribeType Allegiance => entry.Allegiance;
    public bool ForceTribeSelection => entry.ForceTribeSelection;
    public List<int> DiscoveredStats => entry.DiscoveredStats;

    public GameVersion Version => version;

    public override string ToString() => GetLightCharacterData();

    public string GetLightCharacterData()
    {
        var sb = new SeparatedStringBuilder('[');

        sb.Append(GetCharacterInformation());
        sb.Append(Customization);
        sb.Append(Equipment);

        if (Version >= GameVersion.vPets2012)
            sb.Append(PetItemId);

        if (Version >= GameVersion.vMinigames2012)
            sb.Append(Registered ? 1 : 0);

        sb.Append(BuildDiscoveredStats());

        return sb.ToString();
    }

    private string GetCharacterInformation()
    {
        var sb = new SeparatedStringBuilder('<');

        sb.Append(Id);
        sb.Append(CharacterName);

        if (Version >= GameVersion.vLate2013)
            sb.Append(UserUuid);

        sb.Append(Gender);
        sb.Append(MaxLife);
        sb.Append(CurrentLife);
        sb.Append(GlobalLevel);
        sb.Append((int)InteractionStatus);
        sb.Append((int)Allegiance);

        if (Version >= GameVersion.vEarly2014)
            sb.Append(ForceTribeSelection ? 1 : 0);

        return sb.ToString();
    }

    private string BuildDiscoveredStats()
    {
        var sb = new SeparatedStringBuilder('<');

        foreach (var item in DiscoveredStats)
            sb.Append(item);

        return sb.ToString();
    }
}
