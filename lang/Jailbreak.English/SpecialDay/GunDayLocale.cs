using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views.SpecialDay;

namespace Jailbreak.English.SpecialDay;

public class GunDayLocale() : SoloDayLocale("Gun Game",
  "Free for all! Kill players to progress through the weapons.",
  "Players will randomly respawn upon death.",
  "If you get knifed you lose some progress!",
  "Knifing someone lets you skip a gun!"), IGunDayLocale {
  public IView DemotedDueToSuicide
    => new SimpleView { PREFIX, "You were demoted due to dying to yourself." };

  public IView DemotedDueToKnife
    => new SimpleView { PREFIX, "You were demoted due to getting knifed." };

  public IView PromotedTo(string weapon, int weaponsLeft) {
    if (weaponsLeft == 1)
      return new SimpleView {
        PREFIX, "Promoted to", weapon + ".", ChatColors.Green + "LAST WEAPON!"
      };

    return new SimpleView {
      PREFIX,
      "Promoted to",
      weapon + ".",
      weaponsLeft,
      "weapons left."
    };
  }

  public IView PlayerOnLastPromotion(CCSPlayerController player) {
    return new SimpleView {
      PREFIX,
      player,
      "is on the last weapon!",
      ChatColors.LightRed + "Watch out!"
    };
  }

  public IView PlayerWon(CCSPlayerController player) {
    var view = new SimpleView { PREFIX, player, "won the game!" };
    return view;
  }
}