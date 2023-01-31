using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Core.Network.Extensions;
using Server.Reawakened.Levels;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.XMLs;

namespace Server.Reawakened.Players;

public class Player : INetStateData
{
    public Level CurrentLevel;
    public int PlayerId;
    public UserInfo UserInfo;
    public int CurrentCharacter;

    public Player(UserInfo userInfo) => UserInfo = userInfo;

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        if (CurrentLevel != null)
        {
            var levelName = CurrentLevel.LevelData.Name;

            if (!string.IsNullOrEmpty(levelName))
                logger.LogDebug("Dumped player with ID '{User}' from level '{Level}'", PlayerId, levelName);
        }

        CurrentLevel?.DumpPlayerToLobby(PlayerId);
    }

    public void JoinLevel(NetState state, Level level)
    {
        CurrentLevel?.RemoveClient(PlayerId);
        CurrentLevel = level;
        CurrentLevel.AddClient(state);
    }

    public int GetLevelId() => CurrentLevel != null ? CurrentLevel.LevelData.LevelId : -1;

    public CharacterDataModel GetCurrentCharacter()
        => UserInfo.Characters[CurrentCharacter];

    public CharacterDataModel GetCharacterFromName(string characterName)
        => UserInfo.Characters.Values
            .FirstOrDefault(c => c.CharacterName == characterName);
    
    public void SetCharacterSelected(int characterId)
    {
        CurrentCharacter = characterId;
        UserInfo.LastCharacterSelected = GetCurrentCharacter().CharacterName;
    }
    
    public void SendStartPlay(int characterId, NetState state, LevelHandler levelHandler)
    {
        SetCharacterSelected(characterId);
        var level = levelHandler.GetLevelFromId(UserInfo.CharacterLevel[characterId]);
        level?.SendCharacterInfoData(state, this, CharacterInfoType.Detailed);
    }

    public void SetLevel(int levelId, int characterId) => UserInfo.CharacterLevel[characterId] = levelId;

    public void AddCharacter(CharacterDataModel characterData) =>
        UserInfo.Characters.Add(characterData.CharacterId, characterData);
}
