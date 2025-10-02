using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;
using System.Text.RegularExpressions;

namespace Server.Reawakened.Chat.Commands.Misc;
public partial class ChangeName : SlashCommand
{
    public override string CommandName => "/changename";

    public override string CommandDescription => "Change your monkey's name.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "firstName",
            Description = "The monkey's first name.",
            Optional = false
        },
        new ParameterModel()
        {
            Name = "middleName",
            Description = "The monkey's middle name",
            Optional = false
        },
        new ParameterModel()
        {
            Name = "lastName",
            Description = "The monkey's last name",
            Optional = true
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    [GeneratedRegex("[^A-Za-z0-9]+")]
    private static partial Regex MyRegex();

    public CharacterHandler CharacterHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        if (args.Length < 3)
        {
            Log("Please specify a valid name.", player);
            return;
        }

        var names = args.Select(name =>
            MyRegex().Replace(name.ToLower(), string.Empty)
        ).ToList();

        var firstName = names[1];
        var secondName = names[2];

        var thirdName = names.Count > 3 ? names[3] : string.Empty;

        if (firstName.Length > 0)
            firstName = char.ToUpper(firstName[0]) + firstName[1..];

        if (secondName.Length > 0)
            secondName = char.ToUpper(secondName[0]) + secondName[1..];

        var newName = $"{firstName} {secondName}{thirdName}";

        if (CharacterHandler.GetCharacterFromName(newName) != null)
        {
            Log("Please specify a name that is not in use by another _player.", player);
            return;
        }

        player.Character.Write.CharacterName = $"{firstName} {secondName}{thirdName}";

        Log($"You have changed your monkey's name to {player.Character.CharacterName}!", player);
        Log("This change will apply only once you've logged out.", player);
    }
}
