using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Models;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class InternalDialogCatalog : IBundledXml
{
    public string BundleName => "InternalDialogCatalog";
    public bool Priority => false;

    public Dictionary<int, DialogInfo> NpcDialogs;

    public void InitializeVariables() =>
        NpcDialogs = new Dictionary<int, DialogInfo>();

    public void EditDescription(XmlDocument xml, IServiceProvider services)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode childNode in xmlDocument.ChildNodes)
        {
            if (childNode.Name != "dialogCatalog") continue;

            foreach (XmlNode npcs in childNode.ChildNodes)
            {
                if (npcs.Name == "npc")
                {
                    var objectId = -1;
                    var nameId = -1;
                    var descriptionId = -1;

                    var dialog = new Dictionary<int, Conversation>();

                    foreach (XmlAttribute npcInfo in npcs.Attributes!)
                    {
                        switch (npcInfo.Name)
                        {
                            case "objectId":
                                objectId = int.Parse(npcInfo.Value);
                                continue;
                            case "nameId":
                                nameId = int.Parse(npcInfo.Value);
                                continue;
                            case "descriptionId":
                                descriptionId = int.Parse(npcInfo.Value);
                                continue;
                        }
                    }
                    foreach (XmlNode npcChild in npcs.ChildNodes)
                    {
                        if (!(npcChild.Name == "dialog"))
                        {
                            continue;
                        }

                        var dialogId = -1;
                        var conversationId = -1;
                        var minimumReputation = -1;

                        foreach (XmlAttribute id in npcChild.Attributes!)
                        {
                            switch (id.Name)
                            {
                                case "dialogId":
                                    dialogId = int.Parse(id.Value);
                                    continue;
                                case "conversationId":
                                    conversationId = int.Parse(id.Value);
                                    continue;
                                case "minimumReputation":
                                    minimumReputation = int.Parse(id.Value);
                                    continue;
                            }
                        }

                        dialog.TryAdd(minimumReputation, new Conversation(dialogId, conversationId));
                    }

                    if (NpcDialogs.ContainsKey(objectId))
                        continue;

                    NpcDialogs.Add(objectId, new DialogInfo(objectId, nameId, descriptionId, dialog));
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public DialogInfo GetDialogById(int id) =>
        NpcDialogs.TryGetValue(id, out var dialog) ? dialog : null;
}
