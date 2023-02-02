using Server.Reawakened.Players.Enums;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Extensions;

namespace Server.Reawakened.XMLs.Bundles;

public class NameSyllables : NamegenSyllablesXML, IBundledXml
{
    public Dictionary<Gender, List<List<string>>> Syllables;

    public string BundleName => "NamegenSyllabes";

    public void LoadBundle(string xml)
    {
        Syllables = new Dictionary<Gender, List<List<string>>>();

        this.SetPrivateField<NamegenSyllablesXML>("_nameSyllables", new Dictionary<int, List<string>>());

        ReadDescriptionXml(xml);

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
}
