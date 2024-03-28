using AssetRipper.IO.Endian;

namespace Web.AssetBundles.BundleFix.Header.Models;

public interface IEndianReadableWritable : IEndianWritable, IEndianReadable
{
}
