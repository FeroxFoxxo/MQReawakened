using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Reawakened.Players.Services;

namespace Server.Reawakened.Characters.Services;

public class NameChange : IService
{
    private readonly ServerConsole _console;
    private readonly EventSink _sink;
    private readonly ILogger<NameChange> _logger;
    private readonly UserInfoHandler _userInfoHandler;
    private readonly AccountHandler _accountHandler;

    public NameChange(ServerConsole console, EventSink sink,
        ILogger<NameChange> logger, UserInfoHandler userInfoHandler,
        AccountHandler accountHandler)
    {
        _console = console;
        _sink = sink;
        _logger = logger;
        _userInfoHandler = userInfoHandler;
        _accountHandler = accountHandler;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load() =>
        _console.AddCommand(new ConsoleCommand("changeName",
            "Changes the name of a requested user's character.",
            _ => ChangeCharacterName()));

    private void ChangeCharacterName()
    {
        _logger.LogInformation("Please enter the username of whom you wish to edit the name of:");

        var userName = Console.ReadLine()?.Trim();

        var account = _accountHandler.Data.Values.FirstOrDefault(x => x.Username == userName);

        if (account == null)
        {
            _logger.LogError("Could not find user with username: {Username}", userName);
            return;
        }

        var user = _userInfoHandler.Data.Values.FirstOrDefault(x => x.UserId == account.UserId);

        if (user == null)
        {
            _logger.LogError("Could not find user info for account: {AccountId}", account.UserId);
            return;
        }

        _logger.LogInformation("Please select the ID for the character you want to change the name for:");

        foreach (var possibleCharacter in user.Characters)
        {
            _logger.LogInformation("    {CharacterId}: {CharacterName}",
                possibleCharacter.Key, possibleCharacter.Value.CharacterName);
        }

        var id = Console.ReadLine();

        if (!int.TryParse(id, out var intId))
        {
            _logger.LogError("Character Id {CharacterId} is not a number!", id);
            return;
        }

        if (!user.Characters.ContainsKey(intId))
        {
            _logger.LogError("Character list does not contain ID {Id}", id);
            return;
        }

        var character = user.Characters[intId];

        _logger.LogInformation("What would you like to set the character '{CharacterName}''s name to?", character.CharacterName);

        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Character name can not be empty!");
            return;
        }

        character.CharacterName = name;
        user.LastCharacterSelected = name;

        _logger.LogInformation("Successfully set character {Id}'s name to {Name}!", intId, name);
    }
}
