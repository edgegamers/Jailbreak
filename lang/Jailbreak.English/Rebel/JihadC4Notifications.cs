using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Mod.Rebel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jailbreak.English.Rebel;

public class JihadC4Notifications : IJihadC4Notifications, ILanguage<Formatting.Languages.English>
{
    public IView JIHAD_C4_DROPPED => new SimpleView { RebelNotifications.PREFIX, "You dropped your Jihad C4!" };

    public IView JIHAD_C4_PICKUP => new SimpleView { RebelNotifications.PREFIX, "You picked up a Jihad C4!" };

    public IView JIHAD_C4_RECEIVED => new SimpleView { RebelNotifications.PREFIX, "You received a Jihad C4!" };

    public IView PlayerDetonateC4(CCSPlayerController player)
    {
        return new SimpleView { RebelNotifications.PREFIX, $"{player.PlayerName} has detonated a Jihad C4!" };
    }
}
