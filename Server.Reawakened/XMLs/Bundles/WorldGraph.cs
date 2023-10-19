using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;
using WorldGraphDefines;

namespace Server.Reawakened.XMLs.Bundles;

public class WorldGraph : WorldGraphXML, IBundledXml
{
    public string BundleName => "world_graph";
    public BundlePriority Priority => BundlePriority.Low;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public int DefaultLevel;
    public int NewbZone;
    public Dictionary<int, List<DestNode>> WorldGraphNodes;

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<WorldGraphXML>("_worldGraphNodes", new Dictionary<int, List<DestNode>>());
        this.SetField<WorldGraphXML>("_levelNameToID", new Dictionary<string, int>());
        this.SetField<WorldGraphXML>("_levelInfos", new Dictionary<int, LevelInfo>());

        DefaultLevel =
            int.Parse(this.GetField<WorldGraphXML>("CLOCK_TOWER_SQUARE_LEVEL_ID").ToString() ?? string.Empty);
        NewbZone = 452;
        WorldGraphNodes = [];
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle() =>
        WorldGraphNodes = (Dictionary<int, List<DestNode>>)this.GetField<WorldGraphXML>("_worldGraphNodes");

    public DestNode GetDestNodeFromPortal(int levelId, int portalId) =>
        !WorldGraphNodes.TryGetValue(levelId, out var value)
            ? null
            : value.Where(destNode => destNode.PortalID == portalId && destNode.ToLevelID != 0)
        .FirstOrDefault();

    public int GetLevelFromPortal(int levelId, int portalId)
    {
        var node = GetDestNodeFromPortal(levelId, portalId);
        return node == null ? 0 : node.ToLevelID;
    }
}
