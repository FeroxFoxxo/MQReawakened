using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Reflection;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class MiscTextDictionary : LocalizationHandler, IBundledXml
{
    public string BundleName => "MiscTextDict_en-US";
    public BundlePriority Priority => BundlePriority.Highest;

    public Dictionary<int, string> LocalizationDict;

    public MiscTextDictionary() : base(null)
    {
    }

    public void InitializeVariables()
    {
        LocalizationDict = [];

        this.SetField<LocalizationHandler>("_localizationDict", new Dictionary<int, string>());
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadLocalizationXml(xml);

    public void FinalizeBundle()
    {
        var field = typeof(GameGlobals).GetField("_localizationHandler",
                    BindingFlags.Static |
                    BindingFlags.NonPublic);

        field.SetValue(null, this);

        LocalizationDict = (Dictionary<int, string>)this.GetField<LocalizationHandler>("_localizationDict");

        LocalizationDict.TryAdd(0, string.Empty);
    }
}
