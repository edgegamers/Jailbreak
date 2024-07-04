using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;

namespace Jailbreak.English.LastGuard;

public class LastGuardNotifications : ILastGuardNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject PREFIX =
        new HiddenFormatObject($" {ChatColors.Blue}[{ChatColors.LightBlue}Last Guard{ChatColors.Blue}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView LG_STARTED => new SimpleView
    {
        {
            PREFIX,
            $"{ChatColors.Red}Last Guard has been activated!"
        },
    };
}