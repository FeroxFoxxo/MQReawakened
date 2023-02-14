using AssetStudio;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class GetMainAsset
{
    public static string GetMainAssetName(this SerializedFile assetFile, DefaultProgressBar bar)
    {
        var assetBundle = assetFile.ObjectsDic.Values.First(o => o.type == ClassIDType.AssetBundle);

        var dump = assetBundle.Dump();
        var lines = dump.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        var tree = GetTree(lines);

        var baseBundle = tree.FirstOrDefault(a => a.Name == "AssetBundle Base");

        if (baseBundle == null)
        {
            bar.SetMessage($"Bundle '{assetFile.fileName}' does not have base. Returning...");
            return null;
        }

        var mainAsset = baseBundle.SubTrees.FirstOrDefault(a => a.Name == "AssetInfo m_MainAsset");

        if (mainAsset == null)
        {
            bar.SetMessage($"Bundle '{assetFile.fileName}' does not have main asset. Returning...");
            return null;
        }

        var asset = GetAssetString(mainAsset);

        var container = baseBundle.SubTrees.FirstOrDefault(a => a.Name == "map m_Container");

        if (container == null)
        {
            bar.SetMessage($"Bundle '{assetFile.fileName}' does not have map container. Returning...");
            return null;
        }

        var array = container.SubTrees.FirstOrDefault(a => a.Name.StartsWith("int size = "));

        if (array == null)
        {
            bar.SetMessage($"Bundle '{assetFile.fileName}' does not have container size. Returning...");
            return null;
        }

        foreach (var data in array.SubTrees.Where(a => a.Name == "pair data"))
        {
            var dAssetInfo = data.SubTrees.FirstOrDefault(a => a.Name == "AssetInfo second");

            if (GetAssetString(dAssetInfo) != asset)
                continue;

            const string nameStart = "string first = \"";
            var main = data.SubTrees.FirstOrDefault(a => a.Name.StartsWith(nameStart));

            if (main != null)
                return main.Name[nameStart.Length..][..^1];
        }

        throw new InvalidDataException();
    }

    private static string GetAssetString(TreeInfo info) =>
        GenerateStringFromTree(info.SubTrees.FirstOrDefault(a => a.Name == "PPtr<Object> asset"));

    private static string GenerateStringFromTree(TreeInfo tree) =>
        $"{tree.Name}\n{string.Join('\t', tree.SubTrees.Select(GenerateStringFromTree))}";

    private static TreeInfo[] GetTree(IEnumerable<string> tree)
    {
        var info = new List<KeyValuePair<string, List<string>>>();
        KeyValuePair<string, List<string>> pair = default;

        foreach (var treeTxt in tree)
        {
            if (!treeTxt.StartsWith('\t'))
            {
                pair = new KeyValuePair<string, List<string>>(treeTxt, new List<string>());
                if (pair.Key != default)
                    info.Add(pair);
            }
            else if (pair.Key != default)
            {
                pair.Value.Add(treeTxt[1..]);
            }
        }

        return info.Select(i => new TreeInfo(i.Key, GetTree(i.Value))).ToArray();
    }
}
