﻿using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Abstractions;
public interface IInternalBundledXml
{
    public string BundleName { get; }
    public BundlePriority Priority { get; }
    public IServiceProvider Services { get; set; }

    public void InitializeVariables();

    public void EditDescription(XmlDocument xml);

    public void ReadDescription(string xml);

    public void FinalizeBundle();
}
