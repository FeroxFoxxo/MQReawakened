using AssetRipper.IO.Endian;

namespace Web.AssetBundles.BundleFix.Data;

public class FixedAssetFile : IEndianWritable
{
    private readonly byte[] _bundleInfo;

    public uint FileSize => (uint)_bundleInfo.Length;

    public FixedAssetFile(string path) =>
        _bundleInfo = File.ReadAllBytes(path);

    public void Write(EndianWriter writer) => writer.Write(_bundleInfo);
}
