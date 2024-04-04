#nullable enable
using AssetRipper.IO.Endian;
using Server.Reawakened.BundleHost.BundleFix.Header.Enums;
using Server.Reawakened.BundleHost.BundleFix.Header.Models;
using System.Diagnostics;

namespace Server.Reawakened.BundleHost.BundleFix.Header;

/**
 * Structure for Unity asset bundles.
 *
 * @author Nico Bergemann barracuda415@yahoo.de
 * @author FeroxFoxxo feroxfoxxo@gmail.com
 * @unity UnityWebStreamHeader
 */
public abstract record BundleHeader : IEndianReadableWritable
{
    // UnityWeb or UnityRaw
    protected abstract string Signature { get; }

    // File version
    // 3 in Unity 3.5 and 4
    // 2 in Unity 2.6 to 3.4
    // 1 in Unity 1 to 2.5
    public BundleVersion StreamVersion { get; set; }

    // Player version string
    // 2.x.x for Unity 2
    // 3.x.x for Unity 3/4
    public string? UnityVersion { get; set; }

    // Engine version string
    public string? UnityRevision { get; set; }

    protected BundleHeader(UnityVersion version)
    {
        StreamVersion = GetStreamVersion(version);
        UnityVersion = GetUnityVersion(version).ToString();
        UnityRevision = version.ToString();
    }

    public virtual void Read(EndianReader reader)
    {
        var signature = reader.ReadStringZeroTerm();
        Debug.Assert(signature == Signature);
        StreamVersion = (BundleVersion)reader.ReadInt32();
        Debug.Assert(StreamVersion >= 0);
        UnityVersion = reader.ReadStringZeroTerm();
        UnityRevision = reader.ReadStringZeroTerm();
    }

    public virtual void Write(EndianWriter writer)
    {
        writer.WriteStringZeroTerm(Signature);
        writer.Write((int)StreamVersion);
        writer.WriteStringZeroTerm(UnityVersion ??
                                   throw new NullReferenceException(nameof(UnityVersion)));
        writer.WriteStringZeroTerm(UnityRevision ??
                                   throw new NullReferenceException(nameof(UnityRevision)));
    }

    private static BundleVersion GetStreamVersion(UnityVersion version)
    {
        var versionInfo = version.GetVersionInfo();

        return versionInfo switch
        {
            >= 100 and <= 259 => BundleVersion.Bf100250,
            >= 260 and <= 349 => BundleVersion.Bf260340,
            >= 350 and <= 499 => BundleVersion.Bf3504X,
            _ => throw new InvalidDataException($"Invalid stream version: {versionInfo}")
        };
    }

    private static UnityVersion GetUnityVersion(UnityVersion version)
    {
        var engineType = version.Major switch
        {
            2 => 2,
            3 or 4 => 3,
            5 => 5,
            _ => throw new InvalidDataException($"Invalid unity version: {version.Major}")
        };

        return new UnityVersion($"{engineType}.x.x");
    }
}
