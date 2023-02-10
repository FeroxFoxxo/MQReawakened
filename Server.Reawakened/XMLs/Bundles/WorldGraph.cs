using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;
using WorldGraphDefines;

namespace Server.Reawakened.XMLs.Bundles;

public class WorldGraph : WorldGraphXML, IBundledXml
{
    public string BundleName => "world_graph";

    public int ClockTowerId;

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<WorldGraphXML>("_worldGraphNodes", new Dictionary<int, List<DestNode>>());
        this.SetField<WorldGraphXML>("_levelNameToID", new Dictionary<string, int>());
        this.SetField<WorldGraphXML>("_levelInfos", new Dictionary<int, LevelInfo>());

        ClockTowerId = int.Parse(this.GetField<WorldGraphXML>("CLOCK_TOWER_SQUARE_LEVEL_ID").ToString() ?? string.Empty);
    }

    public void EditXml(XmlDocument xml)
    {
    }

    public void ReadXml(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }

    public int GetDestinationFromPortal(int levelId, int portalId)
    {
        var destinationLevelId = 0;

        var worldGraphNodes = (Dictionary<int, List<DestNode>>)this.GetField<WorldGraphXML>("_worldGraphNodes");

        if (worldGraphNodes.TryGetValue(levelId, out var value))
            foreach (var destNode in value.Where(destNode => destNode.PortalID == portalId && destinationLevelId == 0))
                destinationLevelId = destNode.ToLevelID;
        
        return destinationLevelId;
    }
}
