using AssetRipper.IO.Endian;
using Web.AssetBundles.BundleFix.Header.Models;

namespace Web.AssetBundles.BundleFix.Metadata;

public class BundleMetadata : IEndianReadableWritable
{
    public string CacheName;
    public uint FileSize;
    public uint MetadataSize;
    public uint NumberOfScenes;
    public byte Unknown;

    public BundleMetadata(string cacheName, uint fileSize)
    {
        NumberOfScenes = 1;
        CacheName = cacheName;
        MetadataSize = 0;
        FileSize = fileSize;
        Unknown = 0;
    }

    public void Read(EndianReader writer)
    {
        NumberOfScenes = writer.ReadUInt32();

        CacheName = writer.ReadStringZeroTerm();

        MetadataSize = writer.ReadUInt32();
        FileSize = writer.ReadUInt32();
        Unknown = writer.ReadByte();
    }

    public void Write(EndianWriter writer)
    {
        writer.Write(NumberOfScenes);

        writer.WriteStringZeroTerm(CacheName);

        writer.Write(MetadataSize);
        writer.Write(FileSize);
        writer.Write(Unknown);
    }

    public void FixMetadata(uint size) => MetadataSize = size;
}
