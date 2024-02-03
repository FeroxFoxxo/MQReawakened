using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components;

public class TriggerRaceComp : TriggerStatueComp<TriggerRace>
{
    public string StartRaceSpawnPoint => ComponentData.StartRaceSpawnPoint;
    public string[] TargetToEnableDelayedLevelEditorIDs => ComponentData.TargetToEnableDelayedLevelEditorIDs;
    public string[] TargetToResetLevelEditorIDs => ComponentData.TargetToResetLevelEditorIDs;

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {

    }
}
