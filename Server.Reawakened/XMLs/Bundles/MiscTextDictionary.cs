using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;

namespace Server.Reawakened.XMLs.Bundles;

public class MiscTextDictionary : LocalizationHandler, IBundledXml
{
    public Dictionary<int, string> LocalizationDict;

    public MiscTextDictionary(GameClient client) : base(client) => LocalizationDict = new Dictionary<int, string>();

    public string BundleName => "MiscTextDict_en-US";

    public void LoadBundle(string xml)
    {
        this.SetField<LocalizationHandler>("_localizationDict", new Dictionary<int, string>());

        ReadLocalizationXml(xml);

        LocalizationDict = (Dictionary<int, string>) this.GetField<LocalizationHandler>("_localizationDict");
    }
}
