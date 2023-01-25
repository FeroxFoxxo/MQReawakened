using Server.Reawakened.XMLs.Abstractions;
using System.Reflection;

namespace Server.Reawakened.XMLs;

public class NameSyllables : NamegenSyllablesXML, IBundledXml
{
    public Dictionary<int, List<List<string>>> Syllables;

    public string BundleName => "NamegenSyllabes";

    public void LoadBundle(string xml)
    {
        Syllables = new Dictionary<int, List<List<string>>>();

        var wGType = typeof(NamegenSyllablesXML);

        var field = wGType.GetField("_nameSyllables", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(this, new Dictionary<int, List<string>>());

        ReadDescriptionXml(xml);

        Syllables = new Dictionary<int, List<List<string>>>
        {
            {
                0,
                new List<List<string>>
                {
                    GetSyllables(0, true),
                    GetSyllables(1, true),
                    GetSyllables(2, true)
                }
            },
            {
                1,
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
