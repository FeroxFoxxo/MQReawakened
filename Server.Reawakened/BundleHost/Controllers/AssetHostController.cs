﻿using AssetRipper.IO.Endian;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Extensions;
using Server.Reawakened.BundleHost.BundleData.Data;
using Server.Reawakened.BundleHost.BundleData.Header;
using Server.Reawakened.BundleHost.BundleData.Header.Models;
using Server.Reawakened.BundleHost.BundleData.Metadata;
using Server.Reawakened.BundleHost.Configs;
using Server.Reawakened.BundleHost.Extensions;
using Server.Reawakened.BundleHost.Models;
using Server.Reawakened.BundleHost.Services;
using FileIO = System.IO.File;

namespace Server.Reawakened.BundleHost.Controllers;

[Route("/Client/{folder}/{file}")]
public class AssetHostController(BuildAssetList buildAssetList, ILogger<AssetHostController> logger,
    AssetBundleRConfig config, BuildXmlFiles buildXmlList) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetAsset([FromRoute] string folder, [FromRoute] string file)
    {
        var publishConfig = config.PublishConfigs.FirstOrDefault(a => string.Equals(a.Value, file));

        if (!publishConfig.IsDefault())
        {
            logger.LogDebug("Getting Publish Configuration {Type} ({Folder})", publishConfig.Key, folder);
            return Ok(buildAssetList.PublishConfigs[publishConfig.Key]);
        }

        var assetDict = config.AssetDictConfigs.FirstOrDefault(a => string.Equals(a.Value, file));

        if (!assetDict.IsDefault())
        {
            logger.LogDebug("Getting Asset Dictionary {Type} ({Folder})", assetDict.Key, folder);
            return Ok(buildAssetList.AssetDict[assetDict.Key]);
        }

        var name = file.Split('.')[0];

        if (!buildAssetList.InternalAssets.TryGetValue(name, out var asset))
            return NotFound();

        var path = file.EndsWith(".xml")
            ? buildXmlList.XmlFiles.TryGetValue(name, out var xmlFile)
                ? xmlFile
                : throw new FileNotFoundException(
                    $"Could not find: {name}. Did you mean:\n{string.Join('\n', buildXmlList.XmlFiles.Keys)}")
            : await WriteFixedBundleAsync(asset);

        if (config.LogAssetLoadInfo)
            logger.LogDebug("Getting asset {Name} from {File} ({Folder})", asset.Name, path, folder);

        return new FileContentResult(await FileIO.ReadAllBytesAsync(path), "application/octet-stream");
    }

    private async Task<string> WriteFixedBundleAsync(InternalAssetInfo asset)
    {
        var assetName = asset.Name?.Trim();

        var baseDirectory =
            config.DebugInfo
                ? Path.Join(config.BundleSaveDirectory, assetName)
                : config.BundleSaveDirectory;

        InternalDirectory.CreateDirectory(baseDirectory);

        var basePath = Path.Join(baseDirectory, assetName);

        var bundlePath = $"{basePath}.{config.SaveBundleExtension}";

        if (!FileIO.Exists(bundlePath) || config.AlwaysRecreateBundle)
        {
            if (config.LogAssetLoadInfo)
                logger.LogInformation(
                    "Creating Bundle {Name} from {Time} [{Type}]",
                    assetName,
                    DateTime.UnixEpoch.AddSeconds(asset.CacheTime).ToShortDateString(),
                    config.AlwaysRecreateBundle ? "FORCED" : "NOT EXIST"
                );

            using var stream = new MemoryStream();
            var writer = new EndianWriter(stream, EndianType.BigEndian);

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

            // WRITE
            try
            {
                await FileIO.WriteAllBytesAsync(bundlePath, stream.ToArray());
            }
            catch (IOException)
            {

            }

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

        return bundlePath;
    }
}
