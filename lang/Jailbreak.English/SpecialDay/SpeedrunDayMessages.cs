using CounterStrikeSharp.API.Core;
using Jailbreak.Formatting.Base;

namespace Jailbreak.English.SpecialDay;

public class SpeedrunDayMessages() : SoloDayMessages("Speedrunners",
  "Follow the blue player! They will run to a spot on the map.",
  "Each round the slowest players to reach the same",
  "spot will be eliminated") {
  public IView YouAreRunner(int seconds)
    => new SimpleView {
      { SpecialDayMessages.PREFIX, "You are the speedrunner!" },
      SimpleView.NEWLINE, {
        SpecialDayMessages.PREFIX, "You have", seconds,
        "seconds to run to a spot you want players to run towards."
      }
    };

  public IView RunnerAssigned(CCSPlayerController player) {
    return new SimpleView {
      SpecialDayMessages.PREFIX,
      player,
      "is the speedrunner! Follow them closely!"
    };
  }
}