using Server.Reawakened.Players.Modals;

namespace Server.Reawakened.Characters.Extensions;

public static class GetCharacterData
{
    // Will not be current user ID.
    public static string[] GetCharacterLightData(UserInfo user)
    {
        var userId = 0;
        var characterData = "";
        var goId = "";
        var location = "";

        return new string[]
        {
            userId.ToString(),
            characterData,
            goId,
            location
        };
    }
}
