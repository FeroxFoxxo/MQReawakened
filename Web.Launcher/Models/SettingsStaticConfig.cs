using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using System.Dynamic;

namespace Web.Launcher.Models;

public class SettingsStaticConfig : IStaticConfig
{
    public string BaseUrl { get; }
    public bool Fullscreen { get; }
    public bool OnGameClosePopup { get; }

    public SettingsStaticConfig()
    {
        BaseUrl = "http://localhost";
        Fullscreen = false;
        OnGameClosePopup = false;
    }

    public void SetSettings(StartConfig config)
    {
        if (config.GameSettingsFile == null)
            return;

        dynamic settings = JsonConvert.DeserializeObject<ExpandoObject>(File.ReadAllText(config.GameSettingsFile))!;
        settings.launcher.baseUrl = BaseUrl;
        settings.launcher.fullscreen = Fullscreen ? "true" : "false";
        settings.launcher.onGameClosePopup = OnGameClosePopup ? "true" : "false";
        settings.patcher.baseUrl = BaseUrl;
        File.WriteAllText(config.GameSettingsFile, JsonConvert.SerializeObject(settings));
    }
}
