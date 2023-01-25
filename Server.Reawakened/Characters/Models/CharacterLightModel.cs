using A2m.Server;
using System.Text;

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
    public string UserUuid { get; set; }
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
        var array = serverData.Split('[');
        Gender = int.Parse(array[0]);
        Customization = new CharacterCustomDataModel(array[1]);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(CharacterId);
        sb.Append(FieldSeparator);
        sb.Append(CharacterName);
        sb.Append(FieldSeparator);
        sb.Append(UserUuid);
        sb.Append(FieldSeparator);
        sb.Append(Gender);
        sb.Append(FieldSeparator);
        sb.Append(MaxLife);
        sb.Append(FieldSeparator);
        sb.Append(CurrentLife);
        sb.Append(FieldSeparator);
        sb.Append(GlobalLevel);
        sb.Append(FieldSeparator);
        sb.Append(InteractionStatus);
        sb.Append(FieldSeparator);
        sb.Append((int)Allegiance);
        sb.Append(FieldSeparator);
        sb.Append(ForceTribeSelection ? 1 : 0);
        sb.Append(FieldSeparator);

        sb.Append(CharacterDataEndDelimiter);
        sb.Append(Customization);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(Equipment);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(PetItemId);
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(Registered ? "1" : "0");
        sb.Append(CharacterDataEndDelimiter);
        sb.Append(GetDiscoveredStats());
        sb.Append(CharacterDataEndDelimiter);

        return sb.ToString();
    }

    private string GetDiscoveredStats()
    {
        var discoveredStatsString = new StringBuilder();

        foreach (var item in DiscoveredStats)
        {
            discoveredStatsString.Append(item);
            discoveredStatsString.Append(FieldSeparator);
        }

        return discoveredStatsString.ToString();
    }
}
