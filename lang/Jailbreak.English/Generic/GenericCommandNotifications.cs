using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Generic;

public class GenericCommandNotifications : IGenericCommandNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject PREFIX =
        new HiddenFormatObject($" {ChatColors.DarkRed}[{ChatColors.LightRed}JB{ChatColors.DarkRed}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView PlayerNotFound(string query)
    {
        return new SimpleView
            { PREFIX, $"{ChatColors.Red}Player '{ChatColors.LightBlue}{query}{ChatColors.Red}' not found." };
    }

    public IView PlayerFoundMultiple(string query)
    {
        return new SimpleView
        {
            PREFIX,
            $"{ChatColors.Red}Multiple players found for '{ChatColors.LightBlue}{query}{ChatColors.Red}'."
        };
    }

    public IView CommandOnCooldown(DateTime cooldownEndsAt)
    {
        var seconds = (int)(cooldownEndsAt - DateTime.Now).TotalSeconds;
        return new SimpleView
        {
            PREFIX, $"{ChatColors.Grey}Command is on cooldown for", seconds,
            $"{ChatColors.Grey}seconds!"
        };
    }

    public IView InvalidParameter(string parameter, string expected)
    {
        return new SimpleView
        {
            PREFIX,
            $"{ChatColors.Red}Invalid parameter '{ChatColors.LightBlue}{parameter}{ChatColors.Red}', expected a(n) {ChatColors.White}{expected}{ChatColors.Red}."
        };
    }

    public IView NoPermissionMessage(string permission)
    {
        return new SimpleView
        {
            PREFIX,
            $"{ChatColors.Red}You do not have permission to use this command. Required permission: {ChatColors.White}{permission}{ChatColors.Red}."
        };
    }
}