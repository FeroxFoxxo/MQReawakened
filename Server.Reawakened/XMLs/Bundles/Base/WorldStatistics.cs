using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Base;
public class WorldStatistics : StatisticDataXML, IBundledXml
{
    public string BundleName => "WorldStatistics";
    public BundlePriority Priority => BundlePriority.Low;

    public Dictionary<ItemEffectType, Dictionary<WorldStatisticsGroup, Dictionary<int, int>>> Statistics;
    public Dictionary<Globals, float> GlobalStats;
    public Dictionary<ItemEffectType, Dictionary<ItemEffectType, int>> VulnerabilityTable;

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<StatisticDataXML>("_statisticStructure", new Dictionary<ItemEffectType, Dictionary<WorldStatisticsGroup, Dictionary<int, int>>>());
        this.SetField<StatisticDataXML>("_globalAttribute", new Dictionary<Globals, float>());
        this.SetField<StatisticDataXML>("_vulnerabilityTable", new Dictionary<ItemEffectType, Dictionary<ItemEffectType, int>>());

        Statistics = [];
        GlobalStats = [];
        VulnerabilityTable = [];
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
        GameFlow.StatisticData = this;

        Statistics = (Dictionary<ItemEffectType, Dictionary<WorldStatisticsGroup, Dictionary<int, int>>>)this.GetField<StatisticDataXML>("_statisticStructure");
        GlobalStats = (Dictionary<Globals, float>)this.GetField<StatisticDataXML>("_globalAttribute");
        VulnerabilityTable = (Dictionary<ItemEffectType, Dictionary<ItemEffectType, int>>)this.GetField<StatisticDataXML>("_vulnerabilityTable");
    }

    public new int GetValue(ItemEffectType effect, WorldStatisticsGroup group, int level)
    {
        var result = 0;
        if (Statistics.TryGetValue(effect, out var effectValue)
            && effectValue.TryGetValue(group, out var groupValue)
            && groupValue.TryGetValue(level, out var value))
            result = value;
        return result;
    }

    public new float GetGlobalStat(Globals globalsEnumValue)
    {
        var result = 0f;
        if (GlobalStats.TryGetValue(globalsEnumValue, out var value))
            result = value;
        return result;
    }

    public new int GetVulnerabilityRatio(ItemEffectType attackSourceType, ItemEffectType defenseType)
    {
        var result = 0;
        if (VulnerabilityTable.TryGetValue(attackSourceType, out var source) && source.TryGetValue(defenseType, out var value))
            result = value;
        return result;
    }

    public new int GetReputationForNextLevel(int reputation)
    {
        foreach (var key in Statistics[ItemEffectType.IncreaseLevelFromExperience][WorldStatisticsGroup.Player].Keys)
            if (key <= 64 && Statistics[ItemEffectType.IncreaseLevelFromExperience][WorldStatisticsGroup.Player][key] > reputation)
                return Statistics[ItemEffectType.IncreaseLevelFromExperience][WorldStatisticsGroup.Player][key];
        return Statistics[ItemEffectType.IncreaseLevelFromExperience][WorldStatisticsGroup.Player][64];
    }
}
