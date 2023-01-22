namespace Server.Reawakened.XMLs.Abstractions;

public interface IBundledXml
{
    public string BundleName { get; }

    public void LoadBundle(string xml);
}
