using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.Icons.Configs;

public class IconsRConfig : IRConfig
{
    public string[] IconBanks { get; set; }
    public string IconDirectory { get; set; }

    public IconsRConfig()
    {
        IconDirectory = InternalDirectory.GetDirectory("Assets/Icons");
        IconBanks =
        [
            "iconbank_achievements",
            "iconbank_main",
            "iconbank_pets",
            "iconbank_vgmt"
        ];
    }
}
