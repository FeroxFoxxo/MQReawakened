using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class BaseAIState<T> : Component<T>, IAIState
{
    public abstract string StateName { get; }

    public float StartTime = 0;

    public virtual void StartState() { }
    public virtual void UpdateState() { }
    public virtual void StopState() { }

    public virtual ComponentSettings GetSettings() => [];
    private ComponentSettings GetStartSettings() => ["ST", StartTime.ToString()];

    public ComponentSettings GetFullSettings()
    {
        var startSettings = GetStartSettings();
        startSettings.AddRange(GetStartSettings());
        return startSettings;
    }
}
