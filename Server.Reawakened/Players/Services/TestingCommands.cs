using Server.Base.Core.Abstractions;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;
using A2m.Server;
using System;
using Server.Base.Logging;
using Server.Base.Network.Services;
using Server.Reawakened.Network.Extensions;
using Server.Base.Core.Events;

namespace Server.Reawakened.Players.Services;

internal class TestingCommands : IService
{
    private readonly ServerConsole _console;
    private readonly EventSink _sink;
    private readonly ILogger<TestingCommands> _logger;
    private readonly UserInfoHandler _userInfoHandler;
    private readonly AccountHandler _accountHandler;
    private readonly ItemCatalog _itemCatalog;
    private readonly NetStateHandler _netStateHandler;

    public TestingCommands(ServerConsole console, EventSink sink,
        ILogger<TestingCommands> logger, UserInfoHandler userInfoHandler,
        AccountHandler accountHandler, ItemCatalog itemCatalog,
        NetStateHandler netStateHandler)
    {
        _console = console;
        _sink = sink;
        _logger = logger;
        _userInfoHandler = userInfoHandler;
        _accountHandler = accountHandler;
        _itemCatalog = itemCatalog;
        _netStateHandler = netStateHandler;
    }

    public void Initialize() => _sink.WorldLoad += Load;

    public void Load() => _console.AddCommand(new ConsoleCommand("giveItem",
            "Gives an item to a requested user's character.",
            _ => GiveItem()));

    private void GiveItem()
    {
        if (!GetCharacter(out var character, out var user)) return;

        var cache = _itemCatalog.GetField<ItemHandler>("_itemDescriptionCache") as Dictionary<int, ItemDescription>;

        _logger.LogInformation("Enter Item ID:");

        var item = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(item))
        {
            _logger.LogError("Item's id can not be empty!");
            return;
        }

        if (!int.TryParse(item, out var itemId))
        {
            _logger.LogError("Item's id has to be an integer!");
            return;
        }

        _logger.LogInformation("Enter Amount:");

        var c = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(c))
        {
            _logger.LogError("Item's count can not be empty!");
            return;
        }

        if (!int.TryParse(c, out var count))
        {
            _logger.LogError("Item's count has to be an integer!");
            return;
        }

        if (cache.TryGetValue(itemId, out var itemDesc))
        {
            character.Data.Inventory.Items.Add(itemDesc.ItemId, new ItemModel()
            {
                ItemId = itemDesc.ItemId,
                Count = count,
                BindingCount = 0,
                DelayUseExpiry = DateTime.MinValue
            });

            if (_netStateHandler.IsPlayerOnline(user.UserId, out var netState, out var _))
            {
                netState.SendXt("ip", character.Data.Inventory.ToString().Replace('>', '|'), false);
            }
        }
        else _logger.LogError("Could not find item with id {itemId}", itemId);
    }

    private bool GetCharacter(out CharacterModel model, out UserInfo user)
    {
        _logger.LogInformation("Please enter the username of whom you wish to edit:");

        var userName = Console.ReadLine()?.Trim();

        var account = _accountHandler.Data.Values.FirstOrDefault(x => x.Username == userName);
        model = null;
        user = null;

        if (account == null)
        {
            _logger.LogError("Could not find user with username: {Username}", userName);
            return false;
        }

        user = _userInfoHandler.Data.Values.FirstOrDefault(x => x.UserId == account.UserId);

        if (user == null)
        {
            _logger.LogError("Could not find user info for account: {AccountId}", account.UserId);
            return false;
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
            return false;
        }

        if (!user.Characters.ContainsKey(intId))
        {
            _logger.LogError("Character list does not contain ID {Id}", id);
            return false;
        }

        model = user.Characters[intId];

        return true;
    }
}
