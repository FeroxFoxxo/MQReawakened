using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;
using ConversationModel = Server.Reawakened.XMLs.Models.Conversation;

namespace Server.Reawakened.XMLs.Bundles;

public class Dialog : DialogXML, ILocalizationXml
{
    public string BundleName => "Dialog";
    public string LocalizationName => "DialogDict_en-US";

    public Dictionary<string, List<ConversationModel>> QuestDialog;
    public Dictionary<string, List<ConversationModel>> GenericDialog;

    private Dictionary<int, List<Conversation>> _dialogDict;
    private Dictionary<int, string> _dialogNames;

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = true;

        this.SetField<DialogXML>("_localizationDict", new Dictionary<int, string>());
        this.SetField<DialogXML>("_dialogDict", new Dictionary<int, List<Conversation>>());

        _dialogDict = [];
        _dialogNames = [];

        QuestDialog = [];
        GenericDialog = [];
    }

    public void EditLocalization(XmlDocument xml)
    {
    }

    public void ReadLocalization(string xml) => ReadLocalizationXml(xml);

    public void EditDescription(XmlDocument xml, IServiceProvider services)
    {
    }

    public void ReadDescription(string xml)
    {
        ReadDescriptionXml(xml);

        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);

        foreach (XmlNode dialogRoot in xmlDoc.ChildNodes)
        {
            if (!(dialogRoot.Name == "Dialogs"))
                continue;

            foreach (XmlNode dialog in dialogRoot.ChildNodes)
            {
                if (!(dialog.Name == "dialog"))
                    continue;

                var id = -1;
                var dialogName = string.Empty;

                foreach (XmlAttribute attribute in dialog.Attributes!)
                {
                    if (attribute.Name == "id")
                        id = int.Parse(attribute.Value);
                    else if (attribute.Name == "name")
                        dialogName = attribute.Value;
                }

                _dialogNames.TryAdd(id, dialogName);
            }
        }
    }

    public void FinalizeBundle()
    {
        GameFlow.DialogXML = this;

        _dialogDict = this.GetField<DialogXML>("_dialogDict") as Dictionary<int, List<Conversation>>;

        foreach(var dialog in _dialogDict)
        {
            foreach(var conversation in dialog.Value)
            {
                var dialogModel = dialog.Value.Select(c => new ConversationModel(c.DialogId, c.ConversationId)).ToList();

                if (conversation.DialogType == "Quest")
                    QuestDialog.TryAdd(_dialogNames[dialog.Key], dialogModel);
                else if (conversation.DialogType == "Generic")
                    GenericDialog.TryAdd(_dialogNames[dialog.Key], dialogModel);
            }
        }
    }
}
