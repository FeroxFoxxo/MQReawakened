using AssetStudio;
using Newtonsoft.Json;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.Chat.Commands.Moderation;
using System.Collections.Specialized;

namespace Server.Reawakened.BundleHost.Extensions;
public static class WriteScripts
{
    public static void WriteScriptsFromBundle(this SerializedFile assetFile, string assetName, AssetBundleRConfig rConfig)
    {
        var file = Path.Combine(rConfig.ScriptsConfigDirectory, $"{assetName}.json");

        if (File.Exists(file))
        {
            File.Delete(file);
        }

        var scripts = new Dictionary<string, OrderedDictionary>();

        foreach (var behaviour in assetFile.Objects.Where(x => x is MonoBehaviour).Select(x => x as MonoBehaviour))
        {
            behaviour.m_Script.TryGet(out var script);

            if (script == null)
                continue;

            var name = script.m_Name;

            if (scripts.ContainsKey(name))
                continue;

            var type = behaviour.ToType();

            if (type == null)
            {
                var m_Type = behaviour.ConvertToTypeTree(new AssemblyLoader());
                type = behaviour.ToType(m_Type);
            }

            scripts.Add(name, type);
        }

        File.WriteAllText(file, JsonConvert.SerializeObject(scripts, Formatting.Indented));
    }
}
