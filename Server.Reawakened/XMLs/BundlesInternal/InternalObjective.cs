using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalObjective : IBundledXml
{
    public string BundleName => "InternalObjective";
    public BundlePriority Priority => BundlePriority.Highest;

    public Dictionary<int, string> ObjectivePrefabs;

    public InternalObjective()
    {
    }

    public void InitializeVariables() =>
        ObjectivePrefabs = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

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

    public void FinalizeBundle()
    {
    }
}
