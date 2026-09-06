using AssetRipper.IO.Endian;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using Server.Base.Network.Enums;
using Server.Reawakened.BundleHost.BundleData.Data;
using Server.Reawakened.BundleHost.BundleData.Header;
using Server.Reawakened.BundleHost.BundleData.Header.Models;
using Server.Reawakened.BundleHost.BundleData.Metadata;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.BundleHost.Extensions;
using Server.Reawakened.BundleHost.Models;
using Server.Reawakened.Chat.Commands.Moderation;
using System.Collections.Concurrent;
using FileIO = System.IO.File;

namespace Server.Reawakened.BundleHost.Services;

public class GenerateFixedBundles(ILogger<GenerateFixedBundles> logger, EventSink sink, AssetBundleRConfig config,
    ServerConsole console, BuildAssetList buildAssetList, AssetBundleRwConfig rwConfig) : IService
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _bundleLocks = new(StringComparer.OrdinalIgnoreCase);

    public void Initialize() => sink.WorldLoad += Load;

    public void Load() =>
        console.AddCommand(
            "generateFixedBundles",
            "Creates a directory that includes the current version's bundles with fixed metadata.",
            NetworkType.Server,
            _ => WriteFixedBundlesAsync()
        );

    private async void WriteFixedBundlesAsync()
    {
        if (Directory.Exists(config.FixedBundles))
            Directory.Delete(config.FixedBundles, true);

        using var bar = new DefaultProgressBar(
            buildAssetList.InternalAssets.Count,
            "Writing Assets To Disk",
            logger,
            rwConfig
        );

        foreach (var asset in buildAssetList.InternalAssets.Values)
        {
            if (asset == null || string.IsNullOrEmpty(asset.UnityVersion))
                break;

            var assetName = asset.Name?.Trim().ToLower();

            var baseDirectory =
                config.DebugInfo
                    ? Path.Join(config.FixedBundles, assetName)
                    : config.FixedBundles;

            InternalDirectory.CreateDirectory(baseDirectory);

            var basePath = Path.Join(baseDirectory, assetName);

            var bundlePath = $"{basePath}";

            if (!FileIO.Exists(bundlePath) || config.AlwaysRecreateBundle)
            {
                var semaphore = _bundleLocks.GetOrAdd(bundlePath, _ => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();

                try
                {
                    if (FileIO.Exists(bundlePath) && !config.AlwaysRecreateBundle)
                        return;

                    var tempPath = bundlePath + ".tmp";

                    await using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 64, true))
                    {
                        var writer = new EndianWriter(fileStream, EndianType.BigEndian);

                        var unityVersion = new UnityVersion(asset.UnityVersion);

                        var fileName = Path.GetFileName(asset.Path);

                        var data = new FixedAssetFile();
                        await data.ReadAsync(asset.Path);

                        var metadata = new BundleMetadata(fileName, data.FileSize);
                        metadata.FixMetadata((uint)metadata.GetEndianSize());

                        var header = new RawBundleHeader(data.FileSize, metadata.MetadataSize, unityVersion);
                        header.FixHeader((uint)header.GetEndianSize());

                        header.Write(writer);
                        metadata.Write(writer);
                        data.Write(writer);

                        await fileStream.FlushAsync();

                        if (config.DebugInfo)
                        {
                            await FileIO.WriteAllTextAsync($"{basePath}.headerVars", JsonConvert.SerializeObject(header, Formatting.Indented));
                            await FileIO.WriteAllBytesAsync($"{basePath}.header", header.GetEndian());

                            await FileIO.WriteAllTextAsync($"{basePath}.metadataVars",
                                JsonConvert.SerializeObject(metadata, Formatting.Indented));
                            await FileIO.WriteAllBytesAsync($"{basePath}.metadata", metadata.GetEndian());
                            FileIO.Copy(asset.Path!, $"{basePath}.cache", true);
                        }
                    }

                    // Atomic replace
                    if (FileIO.Exists(bundlePath))
                        FileIO.Delete(bundlePath);

                    FileIO.Move(tempPath, bundlePath);
                }
                catch (Exception ex)
                {
                    bar.SetMessage(ex.Message);
                }
                finally
                {
                    semaphore.Release();
                }

                bar.TickBar();
            }
        }

        logger.LogInformation("Finished copying asset bundles...");
    }
}
