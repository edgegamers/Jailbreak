using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Core;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Objects;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Mute;

public class PeaceMessages : IPeaceMessages, ILanguage<Formatting.Languages.English>
{
    private static readonly FormatObject PREFIX =
        new HiddenFormatObject(
            $" {ChatColors.DarkBlue}[{ChatColors.LightBlue}Voice{ChatColors.DarkBlue}]{ChatColors.Grey} ")
        {
            Plain = false,
            Panorama = false,
            Chat = true
        };

    public IView PEACE_ENACTED_BY_ADMIN(int seconds)
    {
        return new SimpleView()
        {
            PREFIX,
            "Peace has been enacted by an admin for",
            seconds,
            "seconds."
        };
    }

    public IView WARDEN_ENACTED_PEACE(int seconds)
    {
        return new SimpleView()
        {
            PREFIX,
            "Warden has enacted peace for",
            seconds,
            "seconds."
        };
    }

    public IView GENERAL_PEACE_ENACTED(int seconds)
    {
        return new SimpleView()
        {
            PREFIX,
            "Peace has been enacted for",
            seconds,
            "seconds."
        };
    }

    public IView UNMUTED_GUARDS => new SimpleView()
    {
        { PREFIX, "Guards have been unmuted." }
    };

    public IView UNMUTED_PRISONERS => new SimpleView()
    {
        { PREFIX, "Prisoners have been unmuted." }
    };

    public IView MUTE_REMINDER => new SimpleView()
    {
        { PREFIX, "You are currently muted!" }
    };

    public IView PEACE_REMINDER => new SimpleView()
    {
        { PREFIX, "Peace is currently active. You should only be talking if absolutely necessary!" }
    };

    public IView DEAD_REMINDER => new SimpleView()
    {
        { PREFIX, "You are dead and cannot speak!" }
    };
    
    public IView ADMIN_DEAD_REMINDER => new SimpleView()
    {
        { PREFIX, "You are dead and should only be talking if absolutely necessary!" }
    };
    
    public IView PEACE_ACTIVE => new SimpleView()
    {
        { PREFIX, "Peace is currently active." }
    };
}