namespace Server.Reawakened.BundleHost.Models;

public class TreeInfo(string name, TreeInfo[] subTrees)
{
    public readonly string Name = name;
    public readonly TreeInfo[] SubTrees = subTrees;
}
