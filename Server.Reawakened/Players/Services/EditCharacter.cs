using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Reawakened.Players.Models;
using Server.Reawakened.XMLs.Bundles;
using SmartFoxClientAPI.Data;

namespace Server.Reawakened.Players.Services;

public class NameChange : IService
{
    private readonly AccountHandler _accountHandler;
    private readonly ServerConsole _console;
    private readonly ILogger<NameChange> _logger;
    private readonly EventSink _sink;
    private readonly UserInfoHandler _userInfoHandler;
    private readonly WorldGraph _worldGraph;

    public NameChange(ServerConsole console, EventSink sink,
        ILogger<NameChange> logger, UserInfoHandler userInfoHandler,
        AccountHandler accountHandler, WorldGraph worldGraph)
    {
        _console = console;
        _sink = sink;
        _logger = logger;
        _userInfoHandler = userInfoHandler;
        _accountHandler = accountHandler;
        _worldGraph = worldGraph;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load()
    {
        _console.AddCommand(new ConsoleCommand("changeName",
            "Changes the name of a requested user's character.",
            _ => ChangeCharacterName()));

        _console.AddCommand(new ConsoleCommand("changeLevel",
            "Changes the level of a requested user's character.",
            _ => ChangeCharacterLevel()));
    }

    private void ChangeCharacterName()
    {
        GetCharacter(out var character, out var user);

        _logger.LogInformation("What would you like to set the character '{CharacterName}''s name to?",
            character.Data.CharacterName);

        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Character name can not be empty!");
            return;
        }

        character.Data.CharacterName = name;
        user.LastCharacterSelected = name;

        _logger.LogInformation("Successfully set character {Id}'s name to {Name}!", character.Data.CharacterId, name);
    }

    private void ChangeCharacterLevel()
    {
        GetCharacter(out var character, out var user);

        _logger.LogInformation("What would you like to set the character '{CharacterName}''s level to?",
            character.Data.CharacterName);
        
        var name = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            _logger.LogError("Character name can not be empty!");
            return;
        }

        character.Data.CharacterName = name;
        user.LastCharacterSelected = name;

        _logger.LogInformation("Successfully set character {Id}'s level to {Name}!", character.Data.CharacterId, name);
    }

    private void GetCharacter(out CharacterModel model, out UserInfo user)
    {
        _logger.LogInformation("Please enter the username of whom you wish to edit the name of:");

        var userName = Console.ReadLine()?.Trim();

        var account = _accountHandler.Data.Values.FirstOrDefault(x => x.Username == userName);
        model = null;
        user = null;

        if (account == null)
        {
            _logger.LogError("Could not find user with username: {Username}", userName);
            return;
        }

        user = _userInfoHandler.Data.Values.FirstOrDefault(x => x.UserId == account.UserId);

        if (user == null)
        {
            _logger.LogError("Could not find user info for account: {AccountId}", account.UserId);
            return;
        }

        _logger.LogInformation("Please select the ID for the character you want to change the name for:");

        foreach (var possibleCharacter in user.Characters)
        {
            _logger.LogInformation("    {CharacterId}: {CharacterName}",
                possibleCharacter.Key, possibleCharacter.Value.Data.CharacterName);
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

        model = user.Characters[intId];
    }
}
