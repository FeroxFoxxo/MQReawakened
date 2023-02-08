using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Reawakened.XMLs.Bundles;
internal class LootsInfo : LootsInfoXML, IBundledXml
{
    public string BundleName => "LootsInfo";

    public void LoadBundle(string xml)
    {
        _rootXmlName = BundleName;
		_hasLocalizationDict = false;

        this.SetField<LootsInfoXML>("_lootsInfoXMLDict", new Dictionary<int, LootsInfoInfo>());

        ReadDescriptionXml(xml);
    }
}
