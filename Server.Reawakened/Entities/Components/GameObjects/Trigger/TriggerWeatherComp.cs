using Server.Reawakened.Entities.Components.GameObjects.Trigger.Abstractions;
using static TriggerWeather;

namespace Server.Reawakened.Entities.Components.GameObjects.Trigger;

public class TriggerWeatherComp : BaseTriggerCoopController<TriggerWeather>
{
    public WeatherType WeatherLevel => ComponentData.WeatherLevel;
    public WeatherGUIEffect ApplyGUIEffect => ComponentData.ApplyGUIEffect;
    public bool ApplyWeatherToRoom => ComponentData.ApplyWeatherToRoom;
}
