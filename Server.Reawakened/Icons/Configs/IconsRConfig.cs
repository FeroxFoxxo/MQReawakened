using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Server.Reawakened.Icons.Configs;

public class IconsRConfig : IRConfig
{
    public string[] IconBanks { get; set; }
    public string IconDirectory { get; set; }
    public string UnknownItemsDirectory { get; set; }
    public string[] IgnoreStarting { get; set; }

    public IconsRConfig()
    {
        IconDirectory = InternalDirectory.GetDirectory("Assets/Icons");
        UnknownItemsDirectory = InternalDirectory.GetDirectory("Assets/UnknownItems");
        IconBanks =
        [
            "iconbank_achievements",
            "iconbank_main",
            "iconbank_pets",
            "iconbank_vgmt"
        ];
        IgnoreStarting = [
            "LV_",
            "NPC_",
            "Ach_",
            "ACH_",
        ];
    }
}
