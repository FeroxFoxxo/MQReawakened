using Server.Reawakened.XMLs.Abstractions.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Abstractions.Interfaces;
public interface IBundledXml
{
    public string BundleName { get; }
    public BundlePriority Priority { get; }

    public void InitializeVariables();
    public void EditDescription(XmlDocument xml);
    public void ReadDescription(string xml);
    public void FinalizeBundle();
}
