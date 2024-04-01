using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Abstractions;

public abstract class InternalXml : IBundledXml
{
    public abstract string BundleName { get; }
    public abstract BundlePriority Priority { get; }

    public abstract void InitializeVariables();

    public void EditDescription(XmlDocument xml) { }
    public void ReadDescription(string xml) { }
    public void FinalizeBundle() { }

    public abstract void ReadDescription(XmlDocument xmlDocument);
}
