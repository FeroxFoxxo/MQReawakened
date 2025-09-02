﻿using Server.Base.Accounts.Enums;
using Server.Base.Database.Accounts;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players;
using Server.Reawakened.XMLs.Data.Commands;

namespace Server.Reawakened.Chat.Commands.Misc;
public class FindPlayer : SlashCommand
{
    public override string CommandName => "/findplayer";

    public override string CommandDescription => "Find a player's id by name.";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "name",
            Description = "Find a player's id by name.",
            Optional = false,
            Options = []
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;

    public CharacterHandler CharacterHandler { get; set; }
    public AccountHandler AccountHandler { get; set; }

    public override void Execute(Player player, string[] args)
    {
        var name = string.Join(" ", args.Skip(1));

        var target = CharacterHandler.GetCharacterFromName(name);

        if (target == null)
        {
            Log("Please provide a valid player name.", player);
            return;
        }

        Log($"{target.CharacterName}'s id is {target.UserUuid}", player);
    }
}
