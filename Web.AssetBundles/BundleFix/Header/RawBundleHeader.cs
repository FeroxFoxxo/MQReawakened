using AssetRipper.IO.Endian;
using Web.AssetBundles.BundleFix.Header.Enums;
using Web.AssetBundles.BundleFix.Header.Models;

namespace Web.AssetBundles.BundleFix.Header;

public record RawBundleHeader : BundleHeader
{
    private readonly uint _fileLength;
    private readonly uint _metadataSize;

    protected override string Signature => "UnityRaw";

    // Minimum number of bytes to read for streamed bundles,
    // equal to completeFileSize for normal bundles
    public uint MinimumStreamedBytes { get; set; }

    // Offset to the bundle data or size of the bundle header
    public int HeaderSize { get; set; }

    // Equal to 1 if it's a streamed bundle, number of levels + mainData assets otherwise
    public int NumberOfScenesToDownloadBeforeStreaming { get; set; }

    // List of compressed and uncompressed offsets
    public BundleScene[] Scenes { get; set; }

    // Equal to file size, sometimes equal to uncompressed data size without the header
    public uint CompleteFileSize { get; set; }

    // Offset to the first asset file within the data area? equals compressed
    // file size if completeFileSize contains the uncompressed data size
    public uint UncompressedBlocksInfoSize { get; set; }

    public RawBundleHeader(uint fileLength, uint metadataSize, UnityVersion version) : base(version)
    {
        // Scene Info
        var sceneLength = fileLength + metadataSize;
        Scenes = new BundleScene[] { new() { CompressedSize = sceneLength, DecompressedSize = sceneLength } };
        NumberOfScenesToDownloadBeforeStreaming = Scenes.Length;

        UncompressedBlocksInfoSize = metadataSize;

        _fileLength = fileLength;
        _metadataSize = metadataSize;

        FixHeader(0);
    }

    public void FixHeader(uint headerSize)
    {
        HeaderSize = (int)headerSize; // Set on concat of header to file

        // Same if one asset
        var totalFileSize = _fileLength + _metadataSize + headerSize;

        MinimumStreamedBytes = totalFileSize;
        CompleteFileSize = totalFileSize;
    }

    public sealed override void Read(EndianReader reader)
    {
        base.Read(reader);

        MinimumStreamedBytes = reader.ReadUInt32();
        HeaderSize = reader.ReadInt32();
        NumberOfScenesToDownloadBeforeStreaming = reader.ReadInt32();
        Scenes = reader.ReadEndianArray<BundleScene>();

        if (HasCompleteFileSize(StreamVersion))
            CompleteFileSize = reader.ReadUInt32();
        if (HasUncompressedBlocksInfoSize(StreamVersion))
            UncompressedBlocksInfoSize = reader.ReadUInt32();

        reader.AlignStream();
    }

    public sealed override void Write(EndianWriter writer)
    {
        base.Write(writer);

        writer.Write(MinimumStreamedBytes);
        writer.Write(HeaderSize);
        writer.Write(NumberOfScenesToDownloadBeforeStreaming);

        writer.WriteEndianArray(Scenes);

        if (HasCompleteFileSize(StreamVersion))
            writer.Write(CompleteFileSize);
        if (HasUncompressedBlocksInfoSize(StreamVersion))
            writer.Write(UncompressedBlocksInfoSize);

        writer.AlignStream();
    }

    /// <summary>
    ///     2.6.0 and greater / Bundle Version 2 +
    /// </summary>
    public static bool HasCompleteFileSize(BundleVersion generation) => generation >= BundleVersion.Bf260340;

    /// <summary>
    ///     3.5.0 and greater / Bundle Version 3 +
    /// </summary>
    public static bool HasUncompressedBlocksInfoSize(BundleVersion generation) => generation >= BundleVersion.Bf3504X;
}
