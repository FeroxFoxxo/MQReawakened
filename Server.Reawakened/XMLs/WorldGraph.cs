using Server.Reawakened.Core.Abstractions;
using System.Reflection;
using WorldGraphDefines;

namespace Server.Reawakened.XMLs;

public class WorldGraph : WorldGraphXML, IBundledXml
{
    public string BundleName => "world_graph";

    public void LoadBundle(string xml)
    {
        _rootXmlName = "world_graph";
        _hasLocalizationDict = false;

        var wGType = typeof(WorldGraphXML);

        var field = wGType.GetField("_worldGraphNodes", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(this, new Dictionary<int, List<DestNode>>());
        field = wGType.GetField("_levelNameToID", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(this, new Dictionary<string, int>());
        field = wGType.GetField("_levelInfos", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(this, new Dictionary<int, LevelInfo>());

        ReadDescriptionXml(xml);
    }
}
