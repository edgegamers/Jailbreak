using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using static Jailbreak.English.Warden.WardenNotifications;

namespace Jailbreak.English.Warden;

public class WardenFireNotifications : IWardenFireNotifications, ILanguage<Formatting.Languages.English>
{
    public IView WARDEN_FIRED =>
        new SimpleView { WARDEN_PREFIX, $"The current warden has been {ChatColors.Red}fired{ChatColors.White} by an admin." };
}
