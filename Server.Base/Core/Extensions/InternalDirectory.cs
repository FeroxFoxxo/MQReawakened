namespace Server.Base.Core.Extensions;

public static class InternalDirectory
{
    private static string _baseInternalDirectory;
    private static string _baseDirectory;

    public static void CreateDirectory(string path)
    {
        if (Directory.Exists(path))
            return;

        if (path != null)
            Directory.CreateDirectory(path);
    }

    public static string GetInternalDirectory(string directoryName)
    {
        var path = Path.Combine(GetBaseInternalDirectory(), directoryName);
        CreateDirectory(path);
        return path;
    }

    public static string GetDirectory(string directoryName)
    {
        var path = Path.Combine(GetBaseDirectory(), directoryName);
        CreateDirectory(path);
        return path;
    }

    public static void Empty(string path)
    {
        var directory = new DirectoryInfo(path);

        foreach (var file in directory.GetFiles())
            file.Delete();

        foreach (var subDirectory in directory.GetDirectories())
            subDirectory.Delete(true);
    }

    public static void OverwriteDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);

        CreateDirectory(path);
    }

    public static string GetBaseDirectory()
    {
        if (_baseDirectory != null) return _baseDirectory;

        _baseDirectory = ConsoleExt.ReadOrEnv("BASE_DIRECTORY", null, Path.GetDirectoryName(GetExePath.Path()));

        return _baseDirectory;
    }

    public static string GetBaseInternalDirectory()
    {
        if (_baseInternalDirectory != null) return _baseInternalDirectory;

        _baseInternalDirectory = ConsoleExt.ReadOrEnv("LOCAL_ASSETS_DIRECTORY", null, Path.GetDirectoryName(GetExePath.Path()));

        return _baseInternalDirectory;
    }
}
