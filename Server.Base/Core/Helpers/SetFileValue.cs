using System.Runtime.InteropServices;

namespace Server.Base.Core.Helpers;

public class SetFileValue
{
    public static string SetIfNotNull(string setting, string title, string filter)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        return string.IsNullOrEmpty(setting)
            ? isWindows
                ? FileDialog.GetFile(title, filter)
                : string.Empty
            : setting;
    }
}
