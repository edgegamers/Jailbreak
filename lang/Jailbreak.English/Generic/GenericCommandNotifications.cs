using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Generic;

public class GenericCommandNotifications : IGenericCommandNotifications, ILanguage<Formatting.Languages.English>
{
    public static FormatObject Prefix =
        new HiddenFormatObject($" {ChatColors.Darkred}[{ChatColors.LightRed}JB{ChatColors.Darkred}]")
        {
            //	Hide in panorama and center text
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView PlayerNotFound(string query)
    {
        return new SimpleView(writer =>
            writer
                .Line(Prefix, $"Player '{query}' not found!"));
    }

    public IView PlayerFoundMultiple(string query)
    {
        return new SimpleView(writer =>
            writer
                .Line(Prefix, $"Multiple players found for '{query}'!"));
    }
}