using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class MiscTextDictionary : LocalizationHandler, IBundledXml
{
    public Dictionary<int, string> LocalizationDict;

    public MiscTextDictionary() : base(null)
    {
    }

    public string BundleName => "MiscTextDict_en-US";

    public void InitializeVariables()
    {
        LocalizationDict = new Dictionary<int, string>();

        this.SetField<LocalizationHandler>("_localizationDict", new Dictionary<int, string>());
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadLocalizationXml(xml);

    public void FinalizeBundle() =>
        LocalizationDict = (Dictionary<int, string>)this.GetField<LocalizationHandler>("_localizationDict");
}
