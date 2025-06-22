using Server.Reawakened.XMLs.Abstractions.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Abstractions.Interfaces;
public interface IBundledXml
{
    string BundleName { get; }
    BundlePriority Priority { get; }

    void InitializeVariables();
    void EditDescription(XmlDocument xml);
    void ReadDescription(string xml);
    void FinalizeBundle();
}
