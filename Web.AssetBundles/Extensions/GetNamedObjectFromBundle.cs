using AssetStudio;
using Object = AssetStudio.Object;

namespace Web.AssetBundles.Extensions;

public static class GetNamedObjectFromBundle
{
    public static GameObject GetGameObject(this List<Object> objects, string assetName) =>
        objects.OfType<GameObject>()
            .FirstOrDefault(x => AreSameAsset(x.m_Name, assetName))
        ?? (assetName.EndsWith("_v2") ? objects.GetGameObject(assetName[..^3]) : null);

    public static AudioClip GetMusic(this IEnumerable<Object> objects, string assetName) =>
        objects.OfType<AudioClip>()
            .FirstOrDefault(x => AreSameAsset(x.m_Name, assetName));

    public static TextAsset GetText(this IEnumerable<Object> objects, string assetName) =>
        objects.OfType<TextAsset>()
            .FirstOrDefault(x => AreSameAsset(x.m_Name, assetName));

    private static bool AreSameAsset(string x, string y) =>
        x.Equals(y, StringComparison.OrdinalIgnoreCase);
}
