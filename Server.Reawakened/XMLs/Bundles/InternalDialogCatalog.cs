using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Models;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class InternalDialogCatalog : IBundledXml
{
    public string BundleName => "InternalDialogCatalog";
    public BundlePriority Priority => BundlePriority.Medium;

    public Microsoft.Extensions.Logging.ILogger Logger { get; set; }
    public IServiceProvider Services { get; set; }

    public Dictionary<int, DialogInfo> NpcDialogs;

    public void InitializeVariables() => NpcDialogs = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var miscDict = Services.GetRequiredService<MiscTextDictionary>();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode childNode in xmlDocument.ChildNodes)
        {
            if (childNode.Name != "DialogCatalog") continue;

            foreach (XmlNode npcs in childNode.ChildNodes)
            {
                if (npcs.Name != "NPC") continue;

                var objectId = -1;
                var name = string.Empty;
                var descriptionId = -1;

                var dialog = new Dictionary<int, Conversation>();

                foreach (XmlAttribute npcInfo in npcs.Attributes!)
                {
                    switch (npcInfo.Name)
                    {
                        case "objectId":
                            objectId = int.Parse(npcInfo.Value);
                            continue;
                        case "name":
                            name = npcInfo.Value;
                            continue;
                        case "descriptionId":
                            descriptionId = int.Parse(npcInfo.Value);
                            continue;
                    }
                }

                foreach (XmlNode npcChild in npcs.ChildNodes)
                {
                    if (!(npcChild.Name == "Dialog")) continue;

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

                var nameModel = miscDict.LocalizationDict.FirstOrDefault(x => x.Value == name);

                if (!string.IsNullOrEmpty(nameModel.Value))
                {
                    if (NpcDialogs.ContainsKey(objectId))
                        continue;

                     NpcDialogs.Add(objectId, new DialogInfo(objectId, nameModel.Key, descriptionId, dialog));
                }
                else
                {
                    Logger.LogError("Cannot find text id for character with name: {Name}", name);
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
