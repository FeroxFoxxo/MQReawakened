namespace Server.Reawakened.Core.Abstractions;

public interface IBundledXml
{
    public string BundleName { get; }

    public void LoadBundle(string xml);
}
