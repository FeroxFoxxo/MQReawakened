namespace Server.Reawakened.Levels.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<T1, T2> OrderDictionary<T1, T2>(this Dictionary<T1, T2> dictionary) =>
        dictionary.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
}
