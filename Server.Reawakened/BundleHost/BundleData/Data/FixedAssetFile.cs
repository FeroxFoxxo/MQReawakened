using AssetRipper.IO.Endian;

namespace Server.Reawakened.BundleHost.BundleData.Data;

public class FixedAssetFile() : IEndianWritable
{
    private byte[] _bundleInfo;

    public async Task ReadAsync(string path) => _bundleInfo = await File.ReadAllBytesAsync(path);

    public uint FileSize => (uint)_bundleInfo.Length;

    public void Write(EndianWriter writer) => writer.Write(_bundleInfo);
}
