using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;

namespace Jailbreak.English.SpecialDay;

public class SpecialDayNotifications : ISpecialDayNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject PREFIX = new HiddenFormatObject($" {ChatColors.DarkRed}[{ChatColors.LightBlue}Special Days{ChatColors.DarkRed}]")
    {
        //	Hide in panorama and center text
        Plain = false,
        Panorama = false,
        Chat = true
    };
    
    public IView SD_WARDAY_STARTED => new SimpleView { PREFIX, "Warday has started! Guards versus Prisoners. Your goal is to ensure that your team is last team standing!" };

    public IView SD_FREEDAY_STARTED => new SimpleView { PREFIX, "Freeday has started! Do whatever you want! Vents/Armory KOS unless allowed by warden" };
    public IView SD_FFA_STARTED => new SimpleView { PREFIX, "Free for all Warday has started! Everyone for themselves." };
    
}