using System.Diagnostics;

namespace Server.Base.Core.Extensions;

public static class GetTicks
{
    public static long TickCount => (long)Ticks;

    private static readonly bool _HighRes = Stopwatch.IsHighResolution;

    private static readonly double _HighFrequency = 1000.0 / Stopwatch.Frequency;
    private const double _LowFrequency = 1000.0 / TimeSpan.TicksPerSecond;

    private static int LinuxMode = -1;

    public static bool IsLinux
    {
        get
        {
            if (LinuxMode == -1)
            {
                var p = (int)Environment.OSVersion.Platform;
                var isLinux = p is 4 or 6 or 128;
                LinuxMode = isLinux ? 1 : 0;
            }

            return LinuxMode == 1;
        }
    }

    public static double Ticks => _HighRes && !IsLinux ?
        Stopwatch.GetTimestamp() * _HighFrequency :
        DateTime.UtcNow.Ticks * _LowFrequency;
}
