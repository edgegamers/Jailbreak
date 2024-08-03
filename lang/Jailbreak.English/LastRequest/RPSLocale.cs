using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;
using Jailbreak.Formatting.Views.LastRequest;

namespace Jailbreak.English.LastRequest;

public class RPSLocale : LastRequestLocale, ILRRPSLocale {
  public IView PlayerMadeChoice(CCSPlayerController player) {
    return new SimpleView { PREFIX, player, "made their choice." };
  }

  public IView BothPlayersMadeChoice() {
    return new SimpleView {
      PREFIX, "Both players have rocked, papered, and scissored! (ew)"
    };
  }

  public IView Tie() {
    return new SimpleView { PREFIX, "It's a tie! Let's go again!" };
  }

  public IView Results(CCSPlayerController guard, CCSPlayerController prisoner,
    int guardPick, int prisonerPick) {
    return new SimpleView {
      PREFIX,
      "Results: ",
      guard,
      " picked ",
      toRPS(guardPick),
      " and ",
      prisoner,
      " picked ",
      toRPS(prisonerPick)
    };
  }

  private string toRPS(int pick) {
    return pick switch {
      0 => "Rock",
      1 => "Paper",
      2 => "Scissors",
      _ => "Unknown"
    };
  }
}