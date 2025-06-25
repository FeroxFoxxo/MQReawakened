using AssetStudio;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.Rooms.Extensions;
using System.Collections.Specialized;
using System.Text.Json;

namespace Server.Reawakened.BundleHost.Extensions;
public static class WriteScripts
{
    public static void GetScriptsFromBundle(this SerializedFile assetFile, string assetName, AssetBundleRConfig rConfig)
    {
        var file = Path.Combine(rConfig.ScriptsConfigDirectory, $"{assetName}.json");

        if (File.Exists(file))
            File.Delete(file);

        var scripts = new Dictionary<string, OrderedDictionary>();
        var blacklistedScripts = new List<string>();

        foreach (var behaviour in assetFile.Objects.Where(x => x is MonoBehaviour).Select(x => x as MonoBehaviour))
        {
            behaviour.m_Script.TryGet(out var script);

            if (script == null)
                continue;

            var name = script.m_Name;

            if (blacklistedScripts.Contains(name))
                continue;

            if (scripts.ContainsKey(name))
            {
                blacklistedScripts.Add(name);
                scripts.Remove(name);
                continue;
            }

            var type = behaviour.ToType();

            if (type == null)
            {
                var m_Type = behaviour.ConvertToTypeTree(new AssemblyLoader());
                type = behaviour.ToType(m_Type);
            }

            var dict = new OrderedDictionary();

            foreach (var key in type.Keys)
                if (!key.ToString().StartsWith("m_"))
                    dict.Add(key, type[key]);

            scripts.Add(name, dict);
        }

        File.WriteAllText(file, JsonSerializer.Serialize(scripts, LoadRoomData._jsonSerializerOptions));
    }
}
