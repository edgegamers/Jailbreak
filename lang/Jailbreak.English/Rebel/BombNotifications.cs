using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Logistics;
using Jailbreak.Formatting.Views;

namespace Jailbreak.English.Rebel;

public class BombNotifications : IBombNotifications, ILanguage<Formatting.Languages.English>
{

    public IView BOMB_DROPPED => new SimpleView { RebelNotifications.PREFIX, "You've dropped your Bomb\u2122 :(" };
    public IView BOMB_PICKUP => new SimpleView { RebelNotifications.PREFIX, "You've picked up The Bomb\u2122!" };
    public IView BOMB_RECEIVED => new SimpleView { RebelNotifications.PREFIX, "You were smugged The Bomb\u2122! Use it wisely." };
    public IView BOMB_USAGE => new SimpleView
    {
	    { RebelNotifications.PREFIX, $"To detonate The Bomb\u2122, hold it out and press {ChatColors.Yellow + "E" + ChatColors.Default}. (or +use)" }, SimpleView.NEWLINE,
	    { RebelNotifications.PREFIX, $"The Bomb\u2122's delay is {ChatColors.Yellow + "2.5 seconds" + ChatColors.Default}." }, SimpleView.NEWLINE,
	    { RebelNotifications.PREFIX, $"You can throw The Bomb\u2122 during the delay period using {ChatColors.Yellow + "G" + ChatColors.Default}."}, SimpleView.NEWLINE,
	    { RebelNotifications.PREFIX, $"You can drop The Bomb\u2122 to other players with {ChatColors.Yellow + "G" + ChatColors.Default}." }
    };

    public IView DETONATING_BOMB(CCSPlayerController player)
    {
        return new SimpleView { RebelNotifications.PREFIX, $"{player.PlayerName} has detonated The Bomb\u2122!" };
    }

    public IView PLAYER_RESULTS(int damage, int kills)
    {
	    return new SimpleView { RebelNotifications.PREFIX, $"The Bomb\u2122 dealt", damage, "HP of damage, and killed", kills, "guards." };
    }
}
