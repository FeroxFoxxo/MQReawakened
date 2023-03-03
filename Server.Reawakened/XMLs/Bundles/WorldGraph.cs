using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;
using WorldGraphDefines;

namespace Server.Reawakened.XMLs.Bundles;

public class WorldGraph : WorldGraphXML, IBundledXml
{
    public int DefaultLevel;
    public string BundleName => "world_graph";

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<WorldGraphXML>("_worldGraphNodes", new Dictionary<int, List<DestNode>>());
        this.SetField<WorldGraphXML>("_levelNameToID", new Dictionary<string, int>());
        this.SetField<WorldGraphXML>("_levelInfos", new Dictionary<int, LevelInfo>());

        DefaultLevel =
            int.Parse(this.GetField<WorldGraphXML>("CLOCK_TOWER_SQUARE_LEVEL_ID").ToString() ?? string.Empty);
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }

    public int GetDestinationFromPortal(int levelId, int portalId)
    {
        var worldGraphNodes = (Dictionary<int, List<DestNode>>)this.GetField<WorldGraphXML>("_worldGraphNodes");

        return !worldGraphNodes.TryGetValue(levelId, out var value)
            ? 0
            : (from destNode in value.Where(destNode => destNode.PortalID == portalId)
                where destNode.ToLevelID != 0
                select destNode.ToLevelID
            ).FirstOrDefault();
    }
}
