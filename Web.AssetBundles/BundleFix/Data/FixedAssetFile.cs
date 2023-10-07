using AssetRipper.IO.Endian;

namespace Web.AssetBundles.BundleFix.Data;

public class FixedAssetFile(string path) : IEndianWritable
{
    private readonly byte[] _bundleInfo = File.ReadAllBytes(path);

    public uint FileSize => (uint)_bundleInfo.Length;

    public void Write(EndianWriter writer) => writer.Write(_bundleInfo);
}
