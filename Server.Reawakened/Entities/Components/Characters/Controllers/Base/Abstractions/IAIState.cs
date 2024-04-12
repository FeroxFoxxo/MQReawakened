namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

public interface IAIState
{
    public void StartState();
    public void UpdateState();
    public void StopState();
}
