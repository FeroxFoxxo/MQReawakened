using Server.Reawakened.BundleHost.Models;
using System.Xml;

namespace Server.Reawakened.BundleHost.Extensions;

public static class ConvertAssetInfo
{
    public static XmlElement ToAssetXml(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = document.CreateElement(name);

        assetXml.SetAttribute("name", asset.Name);
        assetXml.SetAttribute("version", asset.Version.ToString());
        assetXml.SetAttribute("language", Enum.GetName(asset.Locale)?.Replace('_', '-'));
        assetXml.SetAttribute("size", asset.BundleSize.ToString());

        return assetXml;
    }

    public static XmlElement ToPubXml(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = asset.ToAssetXml(name, document);
        assetXml.SetAttribute("type", Enum.GetName(asset.Type));
        return assetXml;
    }

    public static XmlElement ToStoredXml(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = asset.ToPubXml(name, document);

        var pathXml = document.CreateElement("path");
        pathXml.InnerText = asset.Path;
        assetXml.AppendChild(pathXml);

        var versionXml = document.CreateElement("unityVersion");
        versionXml.InnerText = asset.UnityVersion;
        assetXml.AppendChild(versionXml);

        var timeXml = document.CreateElement("cacheTime");
        timeXml.InnerText = asset.CacheTime.ToString();
        assetXml.AppendChild(timeXml);

        return assetXml;
    }

    public static InternalAssetInfo XmlToAsset(this XmlElement assetElement)
    {
        var cacheTime = assetElement.ChildNodes.Cast<XmlNode>()
            .FirstOrDefault(x => x.Name == "cacheTime")
            ?.InnerText;

        return cacheTime != null
            ? new InternalAssetInfo
            {
                Name = assetElement.GetAttribute("name"),
                Version = Convert.ToInt32(assetElement.GetAttribute("version")),
                Type = Enum.Parse<AssetInfo.TypeAsset>(assetElement.GetAttribute("type")),
                Locale = Enum.Parse<RFC1766Locales.LanguageCodes>(assetElement.GetAttribute("language")
                    .Replace('-', '_')),
                BundleSize = Convert.ToInt32(assetElement.GetAttribute("size")),

                Path = assetElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "path")?.InnerText,
                UnityVersion = assetElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "unityVersion")
                    ?.InnerText,
                CacheTime = long.Parse(cacheTime)
            }
            : null;
    }
}
