﻿using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Objects;

namespace Jailbreak.English.SpecialDay;

public class SpecialDayNotifications : ISpecialDayNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject PREFIX =
        new HiddenFormatObject($" {ChatColors.Blue}[{ChatColors.LightBlue}Special Days{ChatColors.Blue}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView SD_WARDAY_STARTED => new SimpleView
    {
        {
            PREFIX,
            $"Today is a {ChatColors.DarkRed}Warday{ChatColors.White}! CTs versus Ts. CTs pick a location on the map, and Ts must actively pursue!"
        },
        { PREFIX, "Last team standing wins!" }
    };

    public IView SD_FREEDAY_STARTED => new SimpleView
    {
        PREFIX,
        $"Today is a {ChatColors.Green}Freeday{ChatColors.White}! Do whatever you want!"
    };

    public IView SD_FFA_STARTED => new SimpleView
        { PREFIX, $"Today is a {ChatColors.LightRed}Free for All{ChatColors.White}! Everyone for themselves." };

    public IView SD_FFA_STARTING => new SimpleView { PREFIX, "Free for all starts in 30 seconds!" };

    public IView SD_CUSTOM_STARTED => new SimpleView
    {
        PREFIX,
        $"Today is a {ChatColors.Yellow}Custom Day{ChatColors.White}, listen carefully to the Warden's instructions."
    };

    public IView SD_NOT_WARDEN => new SimpleView { PREFIX, "You are not the warden." };
    public IView SD_CANT_START => new SimpleView { PREFIX, "Unable to do special days this round." };
}