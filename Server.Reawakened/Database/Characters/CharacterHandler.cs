using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Core.Services;
using Server.Reawakened.Core.Configs;

namespace Server.Reawakened.Database.Characters;
public class CharacterHandler(ServerRConfig serverRConfig, IServiceProvider services, ReawakenedLock dbLock) :
    DataHandler<CharacterDbEntry, ReawakenedDatabase, ReawakenedLock>(services, dbLock)
{
    public override bool HasDefault => false;

    public override CharacterDbEntry CreateDefault() => null;

    public CharacterModel GetCharacterFromId(int id) =>
        GetCharacterFromData(Get(id));

    public CharacterModel GetCharacterFromName(string characterName) =>
        GetCharacterFromId(GetIdFromCharacterName(characterName));

    public CharacterModel GetCharacterFromData(CharacterDbEntry characterData) =>
        characterData != null ? new(characterData, serverRConfig.GameVersion) : null;

    protected int GetIdFromCharacterName(string name)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReawakenedDatabase>();

        lock (DbLock.Lock)
        {
            var character = db.Characters.AsNoTracking().FirstOrDefault(c => c.CharacterName == name);

            return character == null ? -1 : character.Id;
        }
    }

    public bool ContainsCharacterName(string name)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReawakenedDatabase>();

        lock (DbLock.Lock)
            return db.Characters.Any(a => a.CharacterName == name);
    }
}
