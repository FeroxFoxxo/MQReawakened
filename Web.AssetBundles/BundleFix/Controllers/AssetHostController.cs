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
public class AssetHostController : Controller
{
    private readonly BuildAssetList _buildAssetList;
    private readonly BuildXmlFiles _buildXmlList;
    private readonly AssetBundleStaticConfig _config;
    private readonly ILogger<AssetHostController> _logger;
    private readonly ReplaceCaches _replaceCaches;

    public AssetHostController(BuildAssetList buildAssetList, ILogger<AssetHostController> logger,
        AssetBundleStaticConfig config, BuildXmlFiles buildXmlList, ReplaceCaches replaceCaches)
    {
        _buildAssetList = buildAssetList;
        _logger = logger;
        _config = config;
        _buildXmlList = buildXmlList;
        _replaceCaches = replaceCaches;
    }

    [HttpGet]
    public IActionResult GetAsset([FromRoute] string folder, [FromRoute] string file)
    {
        if (_config.KillOnBundleRetry && !file.EndsWith(".xml"))
        {
            var uriPath = $"{folder}/{file}";

            // Don't log to console.
            if (_replaceCaches.CurrentlyLoadedAssets.Contains(uriPath))
                return new StatusCodeResult(StatusCodes.Status418ImATeapot);

            _replaceCaches.CurrentlyLoadedAssets.Add(uriPath);
        }

        var publishConfig = _config.PublishConfigs.FirstOrDefault(a => string.Equals(a.Value, file));

        if (!publishConfig.IsDefault())
        {
            _logger.LogDebug("Getting Publish Configuration {Type} ({Folder})", publishConfig.Key, folder);
            return Ok(_buildAssetList.PublishConfigs[publishConfig.Key]);
        }

        var assetDict = _config.AssetDictConfigs.FirstOrDefault(a => string.Equals(a.Value, file));

        if (!assetDict.IsDefault())
        {
            _logger.LogDebug("Getting Asset Dictionary {Type} ({Folder})", assetDict.Key, folder);
            return Ok(_buildAssetList.AssetDict[assetDict.Key]);
        }

        var name = file.Split('.')[0];

        if (!_buildAssetList.InternalAssets.ContainsKey(name))
            return NotFound();

        var asset = _buildAssetList.InternalAssets[name];

        var path = file.EndsWith(".xml")
            ? _buildXmlList.XmlFiles.TryGetValue(name, out var value)
                ? value
                : throw new FileNotFoundException(
                    $"Could not find: {name}. Did you mean:\n{string.Join('\n', _buildXmlList.XmlFiles.Keys)}")
            : WriteFixedBundle(asset);

        _logger.LogDebug("Getting asset {Name} from {File} ({Folder})", asset.Name, path, folder);

        return new FileContentResult(FileIO.ReadAllBytes(path), "application/octet-stream");
    }

    private string WriteFixedBundle(InternalAssetInfo asset)
    {
        var assetName = asset.Name.Trim();

        var baseDirectory =
            _config.DebugInfo
                ? Path.Join(_config.BundleSaveDirectory, assetName)
                : _config.BundleSaveDirectory;

        Directory.CreateDirectory(baseDirectory);

        var basePath = Path.Join(baseDirectory, assetName);

        var bundlePath = $"{basePath}.{_config.SaveBundleExtension}";

        if (!FileIO.Exists(bundlePath) || _config.AlwaysRecreateBundle)
        {
            _logger.LogInformation("Creating Bundle {Name} [{Type}]", assetName,
                _config.AlwaysRecreateBundle ? "FORCED" : "NOT EXIST");


            using var stream = new MemoryStream();
            var writer = new EndianWriter(stream, EndianType.BigEndian);

            var unityVersion = new UnityVersion(asset.UnityVersion);
            var fileName = Path.GetFileName(asset.Path);

            var data = new FixedAssetFile(_config.UseCacheReplacementScheme ? _config.LocalAssetCache : asset.Path);
            var metadata = new BundleMetadata(fileName, data.FileSize);
            var header = new RawBundleHeader(data.FileSize, metadata.MetadataSize, unityVersion);

            header.FixHeader((uint)header.GetEndianSize());
            metadata.FixMetadata((uint)metadata.GetEndianSize());

            header.Write(writer);
            metadata.Write(writer);
            data.Write(writer);

            // WRITE
            FileIO.WriteAllBytes(bundlePath, stream.ToArray());

            if (_config.DebugInfo)
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
