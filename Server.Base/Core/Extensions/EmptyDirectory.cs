namespace Server.Base.Core.Extensions;

public static class EmptyDirectory
{
    public static void Empty(this DirectoryInfo directory)
    {
        foreach (var file in directory.GetFiles())
            file.Delete();

        foreach (var subDirectory in directory.GetDirectories())
            subDirectory.Delete(true);
    }
}
