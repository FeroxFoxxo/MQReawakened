using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs;

namespace Server.Reawakened.Players.Helpers;

public class NameGenSyllables
{
    private readonly NameSyllables _nameGen;
    private readonly Random _random;

    public NameGenSyllables(NameSyllables nameGen, Random random)
    {
        _nameGen = nameGen;
        _random = random;
    }

    private string GetRandomFromList(IReadOnlyList<string> names) =>
        names[_random.Next(names.Count)];

    public static bool IsNameReserved(string[] names, UserInfoHandler handler)
    {
        var name = $"{names[0]}{names[1]}{names[2]}";
        return handler.Data.Select(a => a.Value.Characters)
            .SelectMany(cl => cl).Any(c => c.CharacterName == name);
    }

    public bool IsPossible(int gender, string[] names) =>
        _nameGen.Syllables[gender][0].Contains(names[0]) &&
        _nameGen.Syllables[gender][1].Contains(names[1]) &&
        _nameGen.Syllables[gender][2].Contains(names[2]);

    public string[] GetRandomName(int gender, UserInfoHandler handler)
    {
        while (true)
        {
            var names = new[]
            {
                GetRandomFromList(_nameGen.Syllables[gender][0]),
                GetRandomFromList(_nameGen.Syllables[gender][1]),
                GetRandomFromList(_nameGen.Syllables[gender][2])
            };

            if (IsNameReserved(names, handler))
                continue;

            return names;
        }
    }
}
