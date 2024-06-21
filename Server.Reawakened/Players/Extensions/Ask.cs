using Microsoft.Extensions.Logging;
using Server.Base.Database.Accounts;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Database.Users;

namespace Server.Reawakened.Players.Extensions;

public static class Ask
{
    public static void GetCharacter(Microsoft.Extensions.Logging.ILogger logger, AccountHandler accountHandler,
        UserInfoHandler userInfoHandler, CharacterHandler characterHandler, out CharacterModel model, out UserInfoModel user)
    {
        logger.LogInformation("Please enter the username of whom you wish to find:");

        var userName = Console.ReadLine()?.Trim();

        var account = accountHandler.GetAccountFromUsername(userName);

        model = null;
        user = null;

        if (account == null)
        {
            logger.LogError("Could not find user with username: {Username}", userName);
            return;
        }

        user = userInfoHandler.GetUserFromId(account.Id);

        if (user == null)
        {
            logger.LogError("Could not find user info for account: {AccountId}", account.Id);
            return;
        }

        if (user.CharacterIds.Count == 0)
        {
            logger.LogError("Could not find any characters for account: {AccountId}", account.Id);
            return;
        }

        logger.LogInformation("Please select the ID for the character you want to run this command for:");

        foreach (var possibleCharacter in user.CharacterIds)
        {
            var character = characterHandler.GetCharacterFromId(possibleCharacter);

            logger.LogInformation("    {CharacterId}: {CharacterName}",
                possibleCharacter, character.Write.CharacterName);
        }

        var id = Console.ReadLine();

        if (!int.TryParse(id, out var intId))
        {
            logger.LogError("Character Id {CharacterId} is not a number!", id);
            return;
        }

        if (!user.CharacterIds.Contains(intId))
        {
            logger.LogError("Character list does not contain ID {Id}", id);
            return;
        }

        model = characterHandler.GetCharacterFromId(intId);
    }
}
