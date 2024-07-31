using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views;
using Jailbreak.Public.Utils;
using Microsoft.Extensions.Primitives;

namespace Jailbreak.English.SpecialDay;

public class InfectionDayMessages() : TeamDayMessages("Infection",
  "CTs are infected and try to kill Ts!") {
  public override IView SpecialDayEnd() {
    var winner = PlayerUtil.GetAlive().FirstOrDefault()?.Team
      ?? CsTeam.Spectator;
    return new SimpleView {
      ISpecialDayMessages.PREFIX,
      Name,
      "ended.",
      (winner == CsTeam.CounterTerrorist ? ChatColors.Blue : ChatColors.Red)
      + (winner == CsTeam.CounterTerrorist ? "Zombies" : "Prisoners"),
      "won!"
    };
  }

  public IView YouWereInfectedMessage(CCSPlayerController? player) {
    return (player == null || !player.IsValid) ?
      new SimpleView {
        ISpecialDayMessages.PREFIX,
        $"{ChatColors.Red}You were {ChatColors.DarkRed}infected{ChatColors.Red}! You are now a zombie!"
      } :
      new SimpleView {
        ISpecialDayMessages.PREFIX,
        $"{ChatColors.Red}You were {ChatColors.DarkRed}infected{ChatColors.Red} by",
        player,
        "! You are now a zombie!"
      };
  }

  public IView InfectedWarning(CCSPlayerController player) {
    return new SimpleView {
      ISpecialDayMessages.PREFIX,
      player,
      $"was {ChatColors.DarkRed}infected{ChatColors.Default}! {ChatColors.Red}Watch out!"
    };
  }
}