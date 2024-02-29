using Server.Reawakened.Entities.AbstractComponents;
using static TriggerWeather;

namespace Server.Reawakened.Entities.Components;

public class TriggerWeatherComp : BaseTriggerCoopController<TriggerWeather>
{
    public WeatherType WeatherLevel => ComponentData.WeatherLevel;
    public WeatherGUIEffect ApplyGUIEffect => ComponentData.ApplyGUIEffect;
    public bool ApplyWeatherToRoom => ComponentData.ApplyWeatherToRoom;
}
