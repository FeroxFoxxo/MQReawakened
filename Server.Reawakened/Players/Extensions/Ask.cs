using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Services;

namespace Server.Reawakened.Players.Extensions;

public static class Ask
{
    public static void GetCharacter(Microsoft.Extensions.Logging.ILogger logger, AccountHandler accountHandler,
        UserInfoHandler userInfoHandler, out CharacterModel model, out UserInfo user)
    {
        logger.LogInformation("Please enter the username of whom you wish to edit:");

        var userName = Console.ReadLine()?.Trim();

        var account = accountHandler.Data.Values.FirstOrDefault(x => x.Username == userName);
        model = null;
        user = null;

        if (account == null)
        {
            logger.LogError("Could not find user with username: {Username}", userName);
            return;
        }

        user = userInfoHandler.Data.Values.FirstOrDefault(x => x.UserId == account.UserId);

        if (user == null)
        {
            logger.LogError("Could not find user info for account: {AccountId}", account.UserId);
            return;
        }

        if (user.Characters.Count == 0)
        {
            logger.LogError("Could not find any characters for account: {AccountId}", account.UserId);
            return;
        }

        logger.LogInformation("Please select the ID for the character you want to change the name for:");

        foreach (var possibleCharacter in user.Characters)
        {
            logger.LogInformation("    {CharacterId}: {CharacterName}",
                possibleCharacter.Key, possibleCharacter.Value.Data.CharacterName);
        }

        var id = Console.ReadLine();

        if (!int.TryParse(id, out var intId))
        {
            logger.LogError("Character Id {CharacterId} is not a number!", id);
            return;
        }

        if (!user.Characters.ContainsKey(intId))
        {
            logger.LogError("Character list does not contain ID {Id}", id);
            return;
        }

        model = user.Characters[intId];
    }
}
