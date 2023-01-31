using A2m.Server;
using Server.Reawakened.Characters.Helpers;
using Server.Reawakened.Core.Models;

namespace Server.Reawakened.Characters.Models;

public class CharacterLightModel
{
    public const char FieldSeparator = '<';
    public const char CharacterDataEndDelimiter = '[';

    public CharacterCustomDataModel Customization { get; set; }
    public EquipmentModel Equipment { get; set; }
    public int PetItemId { get; set; }
    public bool Registered { get; set; }
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public int UserUuid { get; set; }
    public int Gender { get; set; }
    public int MaxLife { get; set; }
    public int CurrentLife { get; set; }
    public int GlobalLevel { get; set; }
    public int InteractionStatus { get; set; }
    public TribeType Allegiance { get; set; }
    public bool ForceTribeSelection { get; set; }
    public HashSet<int> DiscoveredStats { get; set; }

    public CharacterLightModel() {}

    public CharacterLightModel(string serverData)
    {
        var array = serverData.Split(CharacterDataEndDelimiter);

        Gender = int.Parse(array[0]);
        Customization = new CharacterCustomDataModel(array[1]);
        
        Equipment = new EquipmentModel();
        DiscoveredStats = new HashSet<int>();
    }

    public override string ToString() => GetLightCharacterData();

    public int GetGoId() => UserUuid + CharacterId;

    public string GetLightCharacterData()
    {
        var sb = new SeparatedStringBuilder(CharacterDataEndDelimiter);

        sb.Append(GetCharacterInformation());
        sb.Append(Customization);
        sb.Append(Equipment);
        sb.Append(PetItemId);
        sb.Append(Registered ? 1 : 0);
        sb.Append(GetDiscoveredStats());

        return sb.ToString();
    }

    private string GetCharacterInformation()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        sb.Append(CharacterId);
        sb.Append(CharacterName);
        sb.Append(UserUuid);
        sb.Append(Gender);
        sb.Append(MaxLife);
        sb.Append(CurrentLife);
        sb.Append(GlobalLevel);
        sb.Append(InteractionStatus);
        sb.Append((int)Allegiance);
        sb.Append(ForceTribeSelection ? 1 : 0);

        return sb.ToString();
    }

    private string GetDiscoveredStats()
    {
        var sb = new SeparatedStringBuilder(FieldSeparator);

        foreach (var item in DiscoveredStats)
            sb.Append(item);

        return sb.ToString();
    }
}
