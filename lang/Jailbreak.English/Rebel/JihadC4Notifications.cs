

using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using static Jailbreak.English.Generic.GenericCommandNotifications;

namespace Jailbreak.English.Rebel;

public class JihadC4Notifications : IJihadC4Notifications, ILanguage<Formatting.Languages.English>
{

    public IView JihadBombDelaySet(float delay)
    {
        return new SimpleView { GENERIC_PREFIX, $"Delay has been set to {delay} seconds." };
    }

    public IView JIHAD_DELAY_CMD_USAGE =>
        new SimpleView { GENERIC_PREFIX, "Usage: !delay <seconds>" };

    private string formattedCurrentDelay(float delay)
    {
        if (delay == 1f) { return "1 second"; }
        if (delay == 0f) { return "None"; }
        return delay + " seconds";
    }

    public IView JihadCurrentDelay(float delay)
    {
        return new SimpleView { GENERIC_PREFIX, $"Current delay on C4: {formattedCurrentDelay(delay)}" };
    }

    public IView JIHAD_GIVEN_BOMB =>
        new SimpleView { GENERIC_PREFIX, "You have been given the Jihad C4." };

}
