using AssetRipper.IO.Endian;
using Server.Reawakened.BundleHost.BundleData.Header.Models;

namespace Server.Reawakened.BundleHost.BundleData.Metadata;

public class BundleMetadata(string cacheName, uint fileSize) : IEndianReadableWritable
{
    public string CacheName = cacheName;
    public uint FileSize = fileSize;
    public uint MetadataSize = 0;
    public uint NumberOfScenes = 1;
    public byte Unknown = 0;

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
