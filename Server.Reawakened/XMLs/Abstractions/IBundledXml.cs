using System.Xml;

namespace Server.Reawakened.XMLs.Abstractions;

public interface IBundledXml
{
    public string BundleName { get; }

    public void InitializeVariables();

    public void EditXml(XmlDocument xml);

    public void ReadXml(string xml);

    public void FinalizeBundle();
}
