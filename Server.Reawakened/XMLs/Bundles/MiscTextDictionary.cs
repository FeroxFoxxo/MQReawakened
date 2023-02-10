using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class MiscTextDictionary : LocalizationHandler, IBundledXml
{
    public Dictionary<int, string> LocalizationDict;

    public MiscTextDictionary(GameClient client) : base(client) => LocalizationDict = new Dictionary<int, string>();

    public string BundleName => "MiscTextDict_en-US";

    public void InitializeVariables() =>
        this.SetField<LocalizationHandler>("_localizationDict", new Dictionary<int, string>());

    public void EditXml(XmlDocument xml)
    {
    }

    public void ReadXml(string xml) =>
        ReadLocalizationXml(xml);

    public void FinalizeBundle() =>
        LocalizationDict = (Dictionary<int, string>)this.GetField<LocalizationHandler>("_localizationDict");
}
