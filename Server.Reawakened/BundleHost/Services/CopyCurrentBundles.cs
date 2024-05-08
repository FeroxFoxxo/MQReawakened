using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.BundleHost.Models;

namespace Server.Reawakened.BundleHost.Services;

public class CopyCurrentBundles(ILogger<RemoveDuplicates> logger, EventSink sink, AssetBundleRConfig rConfig,
    ServerConsole console, BuildAssetList buildAssetList, AssetBundleRwConfig rwConfig) : IService
{
    public void Initialize() => sink.WorldLoad += Load;

    public void Load() =>
        console.AddCommand(
            "copyCurrentBundles",
            "Creates a directory that includes the current version's bundles.",
            NetworkType.Server,
            _ => CopyCurrentABs()
        );

    private void CopyCurrentABs()
    {
        logger.LogDebug("Emptying copied bundle directory...");
        InternalDirectory.Empty(rConfig.CopiedCurrentBundles);

        logger.LogInformation("Copying asset bundles...");

        using var bar = new DefaultProgressBar(
            buildAssetList.InternalAssets.Count,
            "Writing Assets To Disk",
            logger,
            rwConfig
        );

        foreach (var assets in buildAssetList.InternalAssets)
        {
            try
            {
                var targetDirectory = Path.Combine(rConfig.CopiedCurrentBundles, assets.Key);
                InternalDirectory.CreateDirectory(targetDirectory);

                var sourceDirectory = Path.GetDirectoryName(assets.Value.Path);

                if (sourceDirectory == null)
                    continue;

                foreach (var file in Directory.GetFiles(sourceDirectory))
                    File.Copy(file, Path.Combine(targetDirectory, Path.GetFileName(file)));
            }
            catch (Exception e)
            {
                bar.SetMessage(e.Message);
            }

            bar.TickBar();
        }

        logger.LogInformation("Finished copying asset bundles...");
    }

    public static bool AreFileContentsEqual(string path1, string path2) =>
        File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
}
