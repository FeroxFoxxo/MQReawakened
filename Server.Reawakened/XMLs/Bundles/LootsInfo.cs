using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

public class LootsInfo : LootsInfoXML, IBundledXml<LootsInfo>
{
    public string BundleName => "LootsInfo";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<LootsInfo> Logger { get; set; }
    public IServiceProvider Services { get; set; }

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
