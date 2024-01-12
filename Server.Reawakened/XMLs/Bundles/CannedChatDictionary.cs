using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class CannedChatDictionary : CannedChatXML, ILocalizationXml<CannedChatDictionary>
{
    public string BundleName => "CannedChat";
    public string LocalizationName => "CannedChatDict_en-US";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<CannedChatDictionary> Logger { get; set; }
    public IServiceProvider Services { get; set; }

    private Dictionary<int, string> CannedChatDict;

    public void InitializeVariables()
    {
        CannedChatDict = [];

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
            if (!(phraseRoot.Name == "CannedChatDict"))
                continue;

            foreach (XmlNode phrase in phraseRoot.ChildNodes)
            {
                var id = -1;

                foreach (XmlAttribute attribute in phrase.Attributes)
                {
                    if (attribute.Name == "id")
                        id = int.Parse(attribute.Value);
                }

                var text = phrase.InnerText;

                CannedChatDict.Add(id, text);
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
        CannedChatDict.TryGetValue(id, out var text) ? text : null;
}
