using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles.Base;

public class LootsInfo : LootsInfoXML, IBundledXml
{
    public string BundleName => "LootsInfo";
    public BundlePriority Priority => BundlePriority.Low;

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = false;

        this.SetField<LootsInfoXML>("_lootsInfoXMLDict", new Dictionary<int, LootsInfoInfo>());
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) => ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
    }
}
