using AssetStudio;
using System.Collections.Specialized;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class GetMainAsset
{
    public static string GetMainAssetName(this SerializedFile assetFile, DefaultProgressBar bar)
    {
        if (assetFile.Objects.FirstOrDefault(a => a is AssetBundle) is not AssetBundle assetBundle)
        {
            bar.SetMessage($"Could not find asset bundle for '{assetFile.fileName}'. Returning...");
            return null;
        }

        var type = assetBundle.ToType();

        var mainAsset = type["m_MainAsset"];
        var mainDictionary = mainAsset as OrderedDictionary;

        var assetToFind = GetAsset(mainDictionary);

        var container = type["m_Container"];
        var containerList = container as List<KeyValuePair<object, object>>;

        var foundAsset = containerList.FirstOrDefault(
            x =>
            {
                var name = x.Key as string;
                var dict = x.Value as OrderedDictionary;
                var asset = GetAsset(dict);

                return CompareAsset(asset, assetToFind);
            }
        );

        return foundAsset.Key as string;
    }

    public class Asset(int preloadSize, int fileId, int pathId)
    {
        public int PreloadSize = preloadSize;
        public int FileId = fileId;
        public int PathId = pathId;
    }

    private static Asset GetAsset(OrderedDictionary dictionary)
    {
        var preload = dictionary["preloadSize"];
        var preloadSize = (int)preload;

        var asset = dictionary["asset"];
        var assetDictionary = asset as OrderedDictionary;

        var file = assetDictionary["m_FileID"];
        var fileId = (int)file;

        var path = assetDictionary["m_PathID"];
        var pathId = (int)path;

        return new Asset(preloadSize, fileId, pathId);
    }

    private static bool CompareAsset(Asset asset1, Asset asset2)
        => asset1.PreloadSize == asset2.PreloadSize &&
        asset1.PathId == asset2.PathId && asset1.FileId == asset2.FileId;
}
