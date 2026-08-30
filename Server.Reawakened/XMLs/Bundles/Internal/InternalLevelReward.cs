using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Data.Item;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalLevelReward : InternalXml
{
    public override string BundleName => "InternalLevelReward";
    public override BundlePriority Priority => BundlePriority.Low;

    public Dictionary<int, LevelRewardData> LevelRewardData { get; private set; }

    public override void InitializeVariables() => LevelRewardData = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        var levelNodes = xmlDocument.SelectNodes("/LevelRewardData/Level");

        foreach (XmlNode levelNode in levelNodes)
        {
            var level = 1;
            var prefabName = string.Empty;
            var amount = 1;
                
            foreach (XmlAttribute attribute in levelNode.Attributes)
            {
                switch (attribute.Name)
                {
                    case "level":
                        level = int.Parse(attribute.Value);
                        break;
                    case "prefabName":
                        prefabName = attribute.Value;
                        break;
                    case "amount":
                        amount = int.Parse(attribute.Value);
                        break;
                }
            }
                
            LevelRewardData.Add(level, new LevelRewardData { PrefabName = prefabName, Amount = amount });
        }
    }
}
