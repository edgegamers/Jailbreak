using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using static Jailbreak.English.Generic.GenericCommandNotifications;

namespace Jailbreak.English.Warden;

public class WardenLastGuardNotifications : IWardenLastGuardNotifications, ILanguage<Formatting.Languages.English>
{
    // todo add "prev wardens command are invalid" type thing
    public IView LASTGUARD_ACTIVATED(string wardenDisplayName)
    {
        return new SimpleView { GENERIC_PREFIX, $"{ChatColors.Red}{wardenDisplayName} is claiming Last Guard!" };
    }
        
    public IView LASTGUARD_MAXHEALTH(int maxHealth)
    {
        return new SimpleView { GENERIC_PREFIX, $"{ChatColors.Red}The last guard now has {maxHealth} health."};
    }

    public IView LASTGUARD_TIMELIMIT(int timeInSeconds)
    {
        return new SimpleView 
        {
            { GENERIC_PREFIX, $"{ChatColors.Red}They have {ChatColors.Orange}{timeInSeconds}{ChatColors.Red} seconds to kill the remaining Prisoners until 2 remain." }, SimpleView.NEWLINE,
            { GENERIC_PREFIX, $"{ChatColors.Red}The last 2 Prisoners may LR." }, SimpleView.NEWLINE,
            { GENERIC_PREFIX, $"{ChatColors.Red}Prisoners may hide anywhere within the map." }
        };
    }


}
