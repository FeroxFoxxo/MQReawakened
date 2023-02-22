using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class Dialog : DialogXML, ILocalizationXml
{
    public string BundleName => "Dialog";
    public string LocalizationName => "DialogDict_en-US";

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = true;

        this.SetField<DialogXML>("_localizationDict", new Dictionary<int, string>());
        this.SetField<DialogXML>("_dialogDict", new Dictionary<int, List<Conversation>>());
    }

    public void EditLocalization(XmlDocument xml)
    {
    }

    public void ReadLocalization(string xml) => ReadLocalizationXml(xml);

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) => ReadDescriptionXml(xml);
    
    public void FinalizeBundle()
    {
    }
}
