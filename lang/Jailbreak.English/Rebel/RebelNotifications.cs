﻿using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Rebel;

public class RebelNotifications : IRebelNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject PREFIX =
        new HiddenFormatObject($" {ChatColors.Darkred}[{ChatColors.LightRed}Rebel{ChatColors.Darkred}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView NO_LONGER_REBEL => new SimpleView(writer =>
        writer
            .Line(PREFIX, "You are no longer a rebel."));
}