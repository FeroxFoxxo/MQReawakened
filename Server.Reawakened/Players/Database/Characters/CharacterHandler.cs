using Microsoft.Extensions.Logging;
using Server.Base.Core.Configs;
using Server.Base.Core.Events;
using Server.Base.Core.Services;
using Server.Reawakened.Core.Configs;

namespace Server.Reawakened.Players.Database.Characters;
public class CharacterHandler(EventSink sink, ILogger<CharacterDbEntry> logger,
    InternalRConfig rConfig, InternalRwConfig rwConfig, ServerRConfig serverRConfig) :
    DataHandler<CharacterDbEntry>(sink, logger, rConfig, rwConfig)
{
    public override bool HasDefault => false;

    public override CharacterDbEntry CreateDefault() => null;

    public CharacterModel GetCharacterFromId(int id) =>
        GetCharacterFromData(Get(id));

    public CharacterModel GetCharacterFromName(string characterName) =>
        GetCharacterFromData(GetInternal().FirstOrDefault(c => c.Value.CharacterName == characterName).Value);

    public CharacterModel GetCharacterFromData(CharacterDbEntry characterData) =>
        new(characterData, serverRConfig.GameVersion);
}
