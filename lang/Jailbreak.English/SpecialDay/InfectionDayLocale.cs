using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Public.Utils;

namespace Jailbreak.English.SpecialDay;

public class InfectionDayLocale() : TeamDayLocale("Infection",
  "CTs are infected and try to infect Ts!",
  "Ts can scavenge for any guns, but CTs can only use pistols!") {
  public override IView SpecialDayEnd() {
    var winner = PlayerUtil.GetAlive().FirstOrDefault()?.Team
      ?? CsTeam.Spectator;
    return new SimpleView {
      PREFIX,
      Name,
      "ended.",
      (winner == CsTeam.CounterTerrorist ? ChatColors.Blue : ChatColors.Red)
      + (winner == CsTeam.CounterTerrorist ? "Zombies" : "Prisoners"),
      "won!"
    };
  }

  public IView YouWereInfectedMessage(CCSPlayerController? player) {
    return player == null || !player.IsValid ?
      new SimpleView {
        PREFIX,
        $"{ChatColors.Red}You were {ChatColors.DarkRed}infected{ChatColors.Red}! You are now a zombie!"
      } :
      new SimpleView {
        PREFIX,
        $"{ChatColors.Red}You were {ChatColors.DarkRed}infected{ChatColors.Red} by",
        player,
        "! You are now a zombie!"
      };
  }

  public IView InfectedWarning(CCSPlayerController player) {
    return new SimpleView {
      PREFIX,
      player,
      $"was {ChatColors.DarkRed}infected{ChatColors.Default}! {ChatColors.Red}Watch out!"
    };
  }

  public IView DamageWarning(int seconds) {
    return new SimpleView {
      PREFIX,
      "Damage will be enabled for the",
      CsTeam.Terrorist,
      "team in",
      seconds,
      "seconds."
    };
  }
}