using System.Xml;

namespace Server.Reawakened.XMLs.Abstractions;
public interface IInternalLocalizationXml
{
    string LocalizationName { get; }

    public void EditLocalization(XmlDocument xml);

    public void ReadLocalization(string xml);
}
