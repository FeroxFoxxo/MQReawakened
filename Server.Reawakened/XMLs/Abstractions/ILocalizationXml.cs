namespace Server.Reawakened.XMLs.Abstractions;

public interface ILocalizationXml : IBundledXml
{
    string LocalizationName { get; }

    void ReadLocalization(string xml);
}
