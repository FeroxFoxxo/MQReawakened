using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Base;

public class CannedChatDictionary : CannedChatXML, ILocalizationXml
{
    public string BundleName => "CannedChat";
    public string LocalizationName => "CannedChatDict_en-US";
    public BundlePriority Priority => BundlePriority.Low;

    private Dictionary<int, string> _cannedChatDict;

    public void InitializeVariables()
    {
        _cannedChatDict = [];

        this.SetField<CannedChatXML>("_cannedChatDict", new Dictionary<int, CategoryNode>());
        this.SetField<CannedChatXML>("_dic", new Dictionary<int, string>());
    }

    public void EditLocalization(XmlDocument xml)
    {
    }

    public void ReadLocalization(string xml)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xml);

        foreach (XmlNode phraseRoot in xmlDoc.ChildNodes)
        {
            if (!(phraseRoot.Name == "_cannedChatDict"))
                continue;

            foreach (XmlNode phrase in phraseRoot.ChildNodes)
            {
                var id = -1;

                foreach (XmlAttribute attribute in phrase.Attributes)
                    if (attribute.Name == "id")
                        id = int.Parse(attribute.Value);

                var text = phrase.InnerText;

                _cannedChatDict.Add(id, text);
            }
        }
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) => ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }

    public string GetDialogById(int id) =>
        _cannedChatDict.TryGetValue(id, out var text) ? text : null;
}
