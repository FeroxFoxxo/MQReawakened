using System.Diagnostics;

namespace Server.Base.Core.Extensions;

public static class GetExePath
{
    public static string Path()
    {
        if (EnvironmentExt.IsContainerOrNonInteractive())
            return "/app/out/Init";

        var processModule = Process.GetCurrentProcess().MainModule;
        return processModule != null ? processModule.FileName : string.Empty;
    }
}
