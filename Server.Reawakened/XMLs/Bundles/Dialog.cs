﻿using A2m.Server;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;

internal class Dialog : DialogXML, ILocalizationXml
{
    public string BundleName => "Dialog";

    public string LocalizationName => "DialogDict_en-US";

    public void EditXml(XmlDocument xml) { }
    public void FinalizeBundle() { }
    public void InitializeVariables()
    {
        _rootXmlName = BundleName;
        _hasLocalizationDict = true;

        this.SetField<DialogXML>("_localizationDict", new Dictionary<int, string>());
        this.SetField<DialogXML>("_dialogDict", new Dictionary<int, List<Conversation>>());
    }

    public void ReadXml(string xml) => ReadDescriptionXml(xml);

    public void LoadLocalization(string xml) => ReadLocalizationXml(xml);
}
