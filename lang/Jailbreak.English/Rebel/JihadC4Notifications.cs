using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Rebel;

public class JihadC4Notifications : IJihadC4Notifications, ILanguage<Formatting.Languages.English>
{
    public IView JIHAD_C4_DROPPED => new SimpleView { RebelNotifications.PREFIX, "You dropped your Jihad C4!" };
    public IView JIHAD_C4_PICKUP => new SimpleView { RebelNotifications.PREFIX, "You picked up a Jihad C4!" };
    public IView JIHAD_C4_RECEIVED => new SimpleView { RebelNotifications.PREFIX, "You received a Jihad C4!" };
    public IView JIHAD_C4_USAGE1 => new SimpleView { RebelNotifications.PREFIX, $"To detonate it, hold it out and press {ChatColors.Yellow + "E" + ChatColors.Default}." };
    public IView JIHAD_C4_USAGE2 => new SimpleView { RebelNotifications.PREFIX, $"The default delay is {ChatColors.Yellow + "1 second" + ChatColors.Default}." };
    public IView JIHAD_C4_USAGE3 => new SimpleView { RebelNotifications.PREFIX, $"You can drop the C4 to other players with {ChatColors.Yellow + "G" + ChatColors.Default}." };


    public IView PlayerDetonateC4(CCSPlayerController player)
    {
        return new SimpleView { RebelNotifications.PREFIX, $"{player.PlayerName} has detonated a Jihad C4!" };
    }
}