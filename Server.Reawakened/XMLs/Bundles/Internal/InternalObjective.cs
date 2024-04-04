using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalObjective : InternalXml
{
    public override string BundleName => "InternalObjective";
    public override BundlePriority Priority => BundlePriority.Highest;

    public Dictionary<int, string> ObjectivePrefabs;

    public override void InitializeVariables() =>
        ObjectivePrefabs = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode objectiveXml in xmlDocument.ChildNodes)
        {
            if (!(objectiveXml.Name == "ObjectiveCatalogs")) continue;

            foreach (XmlNode objective in objectiveXml.ChildNodes)
            {
                if (!(objective.Name == "Objective")) continue;

                var prefabName = string.Empty;
                var itemId = -1;

                foreach (XmlAttribute objectiveAttribute in objective.Attributes)
                    switch (objectiveAttribute.Name)
                    {
                        case "prefabName":
                            prefabName = objectiveAttribute.Value;
                            break;
                        case "itemId":
                            itemId = int.Parse(objectiveAttribute.Value);
                            break;
                    }

                ObjectivePrefabs.Add(itemId, prefabName);
            }
        }
    }
}
