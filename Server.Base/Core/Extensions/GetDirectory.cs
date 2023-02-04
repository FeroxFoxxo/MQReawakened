namespace Server.Base.Core.Extensions;

public class GetDirectory
{
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
        Directory.CreateDirectory(path!);
    }
}
