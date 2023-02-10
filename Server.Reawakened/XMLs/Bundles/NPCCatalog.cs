using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Analytics;

namespace Server.Reawakened.XMLs.Bundles;

public class NpcDescription
{
    public readonly int Id;
    public readonly int ObjectId;
    public readonly int NameTextId;
    public readonly NPCController.NPCStatus Status;

    public NpcDescription(int id, int objectId, int nameTextId, NPCController.NPCStatus status)
    {
        Id = id;
        ObjectId = objectId;
        NameTextId = nameTextId;
        Status = status;
    }
}

public class NPCCatalog : IBundledXml
{
    public string BundleName => "NPCCatalog";

    public Dictionary<int, NpcDescription> _cacheNpcs;
    public Dictionary<int, NpcDescription> _cacheNpcByObjectId;

    public NpcDescription GetNpc(int id) => _cacheNpcs.TryGetValue(id, out var npc) ? npc : null;
    public NpcDescription GetNpcByObjectId(int id) => _cacheNpcByObjectId.TryGetValue(id, out var npc) ? npc : null;

    private void ReadDescriptionXml(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode childNode in xmlDocument.ChildNodes)
        {
            if (childNode.Name != "NPCCatalog") continue;
            
            foreach (XmlNode childNode2 in childNode.ChildNodes)
            {
                var id = -1;
                var objectId = -1;
                var nameTextId = -1;
                var status = NPCController.NPCStatus.Unknown;
                    
                if (childNode2.Name == "npc")
                {
                    foreach (XmlAttribute item in childNode2.Attributes!)
                    {
                        switch(item.Name)
                        {
                            case "id":
                                id = int.Parse(item.Value);
                                continue;

                            case "objectid":
                                objectId = int.Parse(item.Value);
                                continue;

                            case "nameid":
                                nameTextId = int.Parse(item.Value);
                                continue;

                            case "vendortype":
                                status = (NPCController.NPCStatus)int.Parse(item.Value);
                                continue;
                        }
                    }
                }

                if (!_cacheNpcs.ContainsKey(id))
                {
                    var npcDesc = new NpcDescription(id, objectId, nameTextId, status);
                    _cacheNpcs.Add(id, npcDesc);
                    _cacheNpcByObjectId.Add(objectId, npcDesc);
                }
            }
        }
    }

    public void InitializeVariables()
    {
        _cacheNpcs = new Dictionary<int, NpcDescription>();
        _cacheNpcByObjectId = new Dictionary<int, NpcDescription>();
    }

    public void EditXml(XmlDocument xml)
    {
    }

    public void ReadXml(string xml) => ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }
}
