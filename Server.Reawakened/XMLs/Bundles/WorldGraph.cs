using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using WorldGraphDefines;

namespace Server.Reawakened.XMLs.Bundles;

public class WorldGraph : WorldGraphXML, IBundledXml
{
    public string BundleName => "world_graph";

    public void LoadBundle(string xml)
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetPrivateField<WorldGraphXML>("_worldGraphNodes", new Dictionary<int, List<DestNode>>());
        this.SetPrivateField<WorldGraphXML>("_levelNameToID", new Dictionary<string, int>());
        this.SetPrivateField<WorldGraphXML>("_levelInfos", new Dictionary<int, LevelInfo>());

        ReadDescriptionXml(xml);
    }
}
