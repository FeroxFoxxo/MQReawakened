using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;
internal class LootsInfo : LootsInfoXML, IBundledXml
{
    public string BundleName => "LootsInfo";

    public void EditXml(XmlDocument xml) { }
    public void FinalizeBundle() { }

    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
		_hasLocalizationDict = false;

        this.SetField<LootsInfoXML>("_lootsInfoXMLDict", new Dictionary<int, LootsInfoInfo>());
    }

    public void ReadXml(string xml) => ReadDescriptionXml(xml);
}
