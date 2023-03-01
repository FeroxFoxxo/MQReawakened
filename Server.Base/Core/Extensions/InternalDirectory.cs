namespace Server.Base.Core.Extensions;

public static class InternalDirectory
{
    private static string _baseDirectory;

    public static void CreateDirectory(string path)
    {
        if (Directory.Exists(path))
            return;

        if (path != null)
            Directory.CreateDirectory(path);
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

        try
        {
            _baseDirectory = GetExePath.Path();

            if (_baseDirectory.Length > 0)
                _baseDirectory = Path.GetDirectoryName(_baseDirectory);
        }
        catch
        {
            _baseDirectory = string.Empty;
        }

        return _baseDirectory;
    }
}
