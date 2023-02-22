using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Models;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class NpcCatalog : IBundledXml
{
    public Dictionary<int, NpcDescription> CachedNpcDict;
    public string BundleName => "NPCCatalog";

    public void InitializeVariables() =>
        CachedNpcDict = new Dictionary<int, NpcDescription>();

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode childNode in xmlDocument.ChildNodes)
        {
            if (childNode.Name != "NPCCatalog") continue;

            foreach (XmlNode childNode2 in childNode.ChildNodes)
            {
                var objectId = -1;
                var nameTextId = -1;
                var status = NPCController.NPCStatus.Unknown;
                var vendorId = -1;

                if (childNode2.Name == "npc")
                    foreach (XmlAttribute item in childNode2.Attributes!)
                    {
                        switch (item.Name)
                        {
                            case "objectId":
                                objectId = int.Parse(item.Value);
                                continue;

                            case "nameId":
                                nameTextId = int.Parse(item.Value);
                                continue;

                            case "vendorType":
                                status = (NPCController.NPCStatus)int.Parse(item.Value);
                                continue;
                            case "vendorId":
                                vendorId = int.Parse(item.Value);
                                continue;
                        }
                    }

                if (CachedNpcDict.ContainsKey(objectId))
                    continue;

                var npcDesc = new NpcDescription(objectId, nameTextId, status, vendorId);
                CachedNpcDict.Add(objectId, npcDesc);
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public NpcDescription GetNpc(int id) =>
        CachedNpcDict.TryGetValue(id, out var npc) ? npc : null;
}
