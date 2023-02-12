using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Network.Services;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Players.Services;

internal class TestingCommands : IService
{
    private readonly AccountHandler _accountHandler;
    private readonly ServerConsole _console;
    private readonly ItemCatalog _itemCatalog;
    private readonly ILogger<TestingCommands> _logger;
    private readonly NetStateHandler _netStateHandler;
    private readonly EventSink _sink;
    private readonly UserInfoHandler _userInfoHandler;

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
        Ask.GetCharacter(_logger, _accountHandler, _userInfoHandler, out var character, out var user);
        
        if (character == null || user == null)
            return;

        _logger.LogInformation("Enter Item ID:");

        var item = Console.ReadLine()?.Trim();
        
        if (!int.TryParse(item, out var itemId))
        {
            _logger.LogError("Item ID has to be an integer");
            return;
        }

        _logger.LogInformation("Enter Amount:");

        var c = Console.ReadLine()?.Trim();
        
        if (!int.TryParse(c, out var count))
        {
            _logger.LogError("Item count has to be an integer");
            return;
        }

        if (_itemCatalog.GetField<ItemHandler>("_itemDescriptionCache") is Dictionary<int, ItemDescription> items &&
            items.TryGetValue(itemId, out var itemDescription))
        {
            character.Data.Inventory.Items.Add(itemDescription.ItemId, new ItemModel
            {
                ItemId = itemDescription.ItemId,
                Count = count,
                BindingCount = 0,
                DelayUseExpiry = DateTime.MinValue
            });

            if (_netStateHandler.IsPlayerOnline(user.UserId, out var netState, out _))
                character.SendUpdatedInventory(netState, false);
        }
        else
        {
            _logger.LogError("Could not find item with ID {ItemId}", itemId);
        }
    }
}
