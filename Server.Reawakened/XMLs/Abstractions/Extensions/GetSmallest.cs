namespace Server.Reawakened.XMLs.Abstractions.Extensions;

public static class GetSmallest
{
    public static int FindSmallest(this List<int> intArray, int lastSmallest)
    {
        var sortedSet = intArray.Where(x => x > lastSmallest).Distinct().OrderBy(x => x).ToArray();
        if (sortedSet.Length == 0) return lastSmallest + 1;
        var smallestMissing = lastSmallest + 1;
        for (var i = 0; i < sortedSet.Length; i++)
        {
            if (smallestMissing < sortedSet[i]) break;
            smallestMissing = sortedSet[i] + 1;
        }
        return smallestMissing;
    }
}
