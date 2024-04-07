using Server.Base.Core.Extensions;
using Server.Reawakened.Players.Enums;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Base;

public class NameSyllables : NamegenSyllablesXML, IBundledXml
{
    public string BundleName => "NamegenSyllabes";
    public BundlePriority Priority => BundlePriority.Low;

    public Dictionary<Gender, List<List<string>>> Syllables;

    public void InitializeVariables() =>
        this.SetField<NamegenSyllablesXML>("_nameSyllables", new Dictionary<int, List<string>>());

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle() =>
        Syllables = new Dictionary<Gender, List<List<string>>>
        {
            {
                Gender.Male,
                new List<List<string>>
                {
                    GetSyllables(0, true),
                    GetSyllables(1, true),
                    GetSyllables(2, true)
                }
            },
            {
                Gender.Female,
                new List<List<string>>
                {
                    GetSyllables(0, false),
                    GetSyllables(1, false),
                    GetSyllables(2, false)
                }
            }
        };
}
