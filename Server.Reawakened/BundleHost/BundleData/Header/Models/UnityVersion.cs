namespace Server.Reawakened.BundleHost.BundleFix.Header.Models;

public class UnityVersion
{
    private readonly string _build;
    private readonly bool _isValid;
    private readonly string _raw;
    private readonly sbyte _unknownByte;
    public readonly sbyte Major;
    public readonly sbyte Minor;
    public readonly sbyte Patch;

    public UnityVersion(string version = "1.0.0f1")
    {
        _unknownByte = Convert.ToSByte(-1);

        try
        {
            Major = PartFromString(version[..1]);
            Minor = PartFromString(version.Substring(2, 1));
            Patch = PartFromString(version.Substring(4, 1));
            _build = version[5..];
            _isValid = true;
        }
        catch (IndexOutOfRangeException)
        {
            _raw = version;
            _isValid = false;
        }
    }

    public int GetVersionInfo() => Major * 100 + Minor * 10 + Patch;

    private sbyte PartFromString(string part) => part == "x" ? _unknownByte : Convert.ToSByte(part);

    private string PartToString(sbyte part) => part == _unknownByte ? "x" : Convert.ToString(part);

    public override string ToString() =>
        _isValid ? $"{PartToString(Major)}.{PartToString(Minor)}.{PartToString(Patch)}{_build}" : _raw;
}
