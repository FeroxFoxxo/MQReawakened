using AssetRipper.IO.Endian;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Extensions;
using Web.AssetBundles.BundleFix.Data;
using Web.AssetBundles.BundleFix.Header;
using Web.AssetBundles.BundleFix.Header.Models;
using Web.AssetBundles.BundleFix.Metadata;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Models;
using Web.AssetBundles.Services;
using FileIO = System.IO.File;

namespace Web.AssetBundles.BundleFix.Controllers;

[Route("/Client/{folder}/{file}")]
public class AssetHostController(BuildAssetList buildAssetList, ILogger<AssetHostController> logger,
    AssetBundleRConfig config, BuildXmlFiles buildXmlList, ReplaceCaches replaceCaches) : Controller
{
    [HttpGet]
    public IActionResult GetAsset([FromRoute] string folder, [FromRoute] string file)
    {
        if (config.KillOnBundleRetry && !file.EndsWith(".xml"))
        {
            var uriPath = $"{folder}/{file}";

            // Don't log to console.
            if (replaceCaches.CurrentlyLoadedAssets.Contains(uriPath))
                return new StatusCodeResult(StatusCodes.Status418ImATeapot);

            replaceCaches.CurrentlyLoadedAssets.Add(uriPath);
        }

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
            : WriteFixedBundle(asset);

        if (config.LogAssetLoadInfo)
            logger.LogDebug("Getting asset {Name} from {File} ({Folder})", asset.Name, path, folder);

        return new FileContentResult(FileIO.ReadAllBytes(path), "application/octet-stream");
    }

    private string WriteFixedBundle(InternalAssetInfo asset)
    {
        var assetName = asset.Name.Trim();

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
            var data = new FixedAssetFile(asset.Path);

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
                FileIO.WriteAllBytes(bundlePath, stream.ToArray());
            }
            catch (IOException)
            {

            }

            if (config.DebugInfo)
            {
                FileIO.WriteAllText($"{basePath}.headerVars", JsonConvert.SerializeObject(header, Formatting.Indented));
                FileIO.WriteAllBytes($"{basePath}.header", header.GetEndian());

                FileIO.WriteAllText($"{basePath}.metadataVars",
                    JsonConvert.SerializeObject(metadata, Formatting.Indented));
                FileIO.WriteAllBytes($"{basePath}.metadata", metadata.GetEndian());
                FileIO.Copy(asset.Path!, $"{basePath}.cache", true);
            }
        }

        return bundlePath;
    }
}
