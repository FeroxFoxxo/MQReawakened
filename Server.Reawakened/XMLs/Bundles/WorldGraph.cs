using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using WorldGraphDefines;

namespace Server.Reawakened.XMLs.Bundles;

public class WorldGraph : WorldGraphXML, IBundledXml
{
    public string BundleName => "world_graph";

    public int ClockTowerId;

    public void LoadBundle(string xml)
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<WorldGraphXML>("_worldGraphNodes", new Dictionary<int, List<DestNode>>());
        this.SetField<WorldGraphXML>("_levelNameToID", new Dictionary<string, int>());
        this.SetField<WorldGraphXML>("_levelInfos", new Dictionary<int, LevelInfo>());

        ClockTowerId = int.Parse(this.GetField<WorldGraphXML>("CLOCK_TOWER_SQUARE_LEVEL_ID").ToString() ?? string.Empty);

        ReadDescriptionXml(xml);
    }
}
