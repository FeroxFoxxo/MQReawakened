﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Models.Npcs;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalDialog : IBundledXml<InternalDialog>
{
    public string BundleName => "InternalDialog";
    public BundlePriority Priority => BundlePriority.Medium;

    public ILogger<InternalDialog> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    // <Level Id, <Npc Id, Dialog Info>>
    public Dictionary<int, Dictionary<int, DialogInfo>> NpcDialogs;

    public void InitializeVariables() => NpcDialogs = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var miscDict = Services.GetRequiredService<MiscTextDictionary>();

        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode dialogXml in xmlDocument.ChildNodes)
        {
            if (dialogXml.Name != "DialogCatalog") continue;

            foreach (XmlNode level in dialogXml.ChildNodes)
            {
                if (level.Name != "Level") continue;

                var levelId = -1;

                foreach (XmlAttribute levelAttribute in level.Attributes)
                    switch (levelAttribute.Name)
                    {
                        case "id":
                            levelId = int.Parse(levelAttribute.Value);
                            continue;
                    }

                if (!NpcDialogs.ContainsKey(levelId))
                    NpcDialogs.Add(levelId, []);

                foreach (XmlNode npc in level.ChildNodes)
                {
                    if (npc.Name != "NPC") continue;

                    var objectId = -1;
                    var name = string.Empty;
                    var nameId = -1;
                    var descriptionId = -1;

                    var dialogList = new Dictionary<int, ConversationInfo>();

                    foreach (XmlAttribute npcAttribute in npc.Attributes)
                        switch (npcAttribute.Name)
                        {
                            case "objectId":
                                objectId = int.Parse(npcAttribute.Value);
                                continue;
                            case "name":
                                name = npcAttribute.Value;
                                continue;
                            case "nameId":
                                nameId = int.Parse(npcAttribute.Value);
                                continue;
                            case "descriptionId":
                                descriptionId = int.Parse(npcAttribute.Value);
                                continue;
                        }

                    foreach (XmlNode dialog in npc.ChildNodes)
                    {
                        if (!(dialog.Name == "Dialog")) continue;

                        var dialogId = -1;
                        var conversationId = -1;
                        var minimumReputation = -1;

                        foreach (XmlAttribute dialogAttribute in dialog.Attributes)
                            switch (dialogAttribute.Name)
                            {
                                case "dialogId":
                                    dialogId = int.Parse(dialogAttribute.Value);
                                    continue;
                                case "conversationId":
                                    conversationId = int.Parse(dialogAttribute.Value);
                                    continue;
                                case "minimumReputation":
                                    minimumReputation = int.Parse(dialogAttribute.Value);
                                    continue;
                            }

                        dialogList.TryAdd(minimumReputation, new ConversationInfo(dialogId, conversationId));
                    }

                    var nameModel = nameId > 0 ?
                        miscDict.LocalizationDict.FirstOrDefault(x => x.Key == nameId) :
                        miscDict.LocalizationDict.FirstOrDefault(x => x.Value == name);

                    if (!string.IsNullOrEmpty(nameModel.Value))
                    {
                        if (NpcDialogs.ContainsKey(objectId))
                            continue;

                        NpcDialogs[levelId].Add(objectId, new DialogInfo(objectId, nameModel.Key, descriptionId, dialogList));
                    }
                    else
                        Logger.LogError("Cannot find text id for character with name: {Name}", nameId > 0 ? nameId : name);
                }
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public DialogInfo GetDialogById(int levelId, int dialogId) =>
        NpcDialogs.TryGetValue(levelId, out var levelInfo) ?
        levelInfo.TryGetValue(dialogId, out var dialog) ?
        dialog :
        null :
        null;
}
