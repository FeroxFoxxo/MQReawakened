
namespace Server.Reawakened.Entities.AbstractComponents;

public class TriggerStatueComp<T> : TriggerCoopControllerComp<T>, IStatueComp where T : TriggerStatue
{
    List<string> IStatueComp.CurrentPhysicalInteractors { get => CurrentPhysicalInteractors; set => CurrentPhysicalInteractors = value; }
}
