using Microsoft.Extensions.Logging;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;

namespace Server.Reawakened.Players.Services;
public class CharacterHandler(EventSink sink, ILogger<CharacterModel> logger,
    InternalRConfig rConfig, InternalRwConfig rwConfig, ServerRConfig serverRConfig) :
    DataHandler<CharacterModel>(sink, logger, rConfig, rwConfig)
{
    public override bool HasDefault => false;

    public override CharacterModel CreateDefault() => null;

    public CharacterModel Create(CharacterDataModel cData, LevelData lData)
    {
        var character = new CharacterModel()
        {
            Data = cData,
            LevelData = lData
        };

        Add(character);

        character.Data.SetCharacterId(character.Id);

        return character;
    }

    public override CharacterModel Get(int id)
    {
        var character = base.Get(id);
        character.Data.SetVersion(serverRConfig.GameVersion);
        return character;
    }

    public CharacterModel GetCharacterFromName(string characterName)
    {
        var character = GetInternal().FirstOrDefault(c => c.Value.Data.CharacterName == characterName).Value;
        character.Data.SetVersion(serverRConfig.GameVersion);
        return character;
    }
}
