using AssetRipper.IO.Endian;
using Server.Reawakened.BundleHost.BundleFix.Header.Models;

namespace Server.Reawakened.BundleHost.Extensions;

public static class EndianExtensions
{
    public static long GetEndianSize(this IEndianReadableWritable endian)
    {
        using var stream = new MemoryStream();

        var writer = new EndianWriter(stream, EndianType.BigEndian);
        var reader = new EndianReader(stream, EndianType.BigEndian);

        var initPosition = stream.Position;
        endian.Write(writer);
        stream.Position = initPosition;

        var basePosition = stream.Position;
        endian.Read(reader);

        return stream.Position - basePosition;
    }

    public static byte[] GetEndian(this IEndianWritable endian)
    {
        using var stream = new MemoryStream();
        using var writer = new EndianWriter(stream, EndianType.BigEndian);
        endian.Write(writer);
        return stream.ToArray();
    }
}
