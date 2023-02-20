using AssetStudio;
using System.Text;
using System.Xml;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class GetData
{
    public static string GetXmlData(this InternalAssetInfo asset, DefaultProgressBar bar)
    {
        try
        {
            var file = File.ReadAllText(asset.Path);

            if (file.FirstOrDefault() == '<')
                try
                {
                    new XmlDocument().LoadXml(file);
                    return file;
                }
                catch (XmlException)
                {
                }

            var manager = new AssetsManager();

            manager.LoadFiles(asset.Path);

            var textAsset = manager.assetsFileList.First().ObjectsDic.Values.GetText(asset.Name);

            var text = Encoding.UTF8.GetString(textAsset.m_Script);
            var length = text.Split('\n').Length;

            bar.SetMessage($"Read XML {asset.Name} for {length} lines.");

            return text;
        }
        catch (Exception e)
        {
            bar.SetMessage($"{asset.Name} could not load! Exception: {e}. Skipping...");
            return string.Empty;
        }
    }
}
